using AbstractBot;
using AbstractBot.Bots;
using AbstractBot.Configs.MessageTemplates;
using DaresGameBot.Configs;
using DaresGameBot.Game.Data;
using DaresGameBot.Game.Data.Cards;
using DaresGameBot.Game.Matchmaking;
using DaresGameBot.Operations;
using DaresGameBot.Operations.Commands;
using GoogleSheetsManager.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AbstractBot.Operations.Data;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using DaresGameBot.Game.Data.PlayerListUpdates;
using DaresGameBot.Game.Matchmaking.Interactions;
using DaresGameBot.Operations.Info;

namespace DaresGameBot;

public sealed class Bot : BotWithSheets<Config, Texts, object, CommandDataSimple>
{
    public Bot(Config config) : base(config)
    {
        Operations.Add(new UpdateCommand(this));
        Operations.Add(new NewCommand(this));
        Operations.Add(new LangCommand(this));
        Operations.Add(new UpdatePlayers(this));
        Operations.Add(new RevealCard(this));
        Operations.Add(new CompleteCard(this));

        GoogleSheetsManager.Documents.Document document = DocumentsManager.GetOrAdd(Config.GoogleSheetId);

        _actionsSheet = document.GetOrAddSheet(Config.Texts.ActionsTitle);
        _questionsSheet = document.GetOrAddSheet(Config.Texts.QuestionsTitle);
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken);

        Chat chat = new()
        {
            Id = Config.LogsChatId,
            Type = ChatType.Private
        };

        await UpdateDecksAsync(chat);
    }

    internal async Task UpdateDecksAsync(Chat chat)
    {
        await using (await StatusMessage.CreateAsync(this, chat, Config.Texts.ReadingDecks))
        {
            List<Game.Data.Cards.Action> actions =
                await _actionsSheet.LoadAsync<Game.Data.Cards.Action>(Config.ActionsRange);
            List<Question> questions = await _questionsSheet.LoadAsync<Question>(Config.QuestionsRange);

            _decksProvider = new Game.DecksProvider(actions, questions);
        }

        Contexts.Remove(chat.Id);
    }

    internal Task UpdatePlayersAsync(Chat chat, User sender, List<PlayerListUpdate> updates)
    {
        Game.Data.Game? game = TryGetContext<Game.Data.Game>(sender.Id);
        if (game is null)
        {
            game = StartNewGame(updates);
            Contexts[sender.Id] = game;
            return ReportNewGameAsync(chat, game);
        }
        ushort pointsForNewPlayers = game.UpdatePlayers(updates);

        MessageTemplateText playersText =
            Config.Texts.PlayersFormat.Format(string.Join(PlayerSeparator, game.Players));

        MessageTemplateText? points = null;
        if (pointsForNewPlayers > 0)
        {
            points = Config.Texts.PoinsForNewPlayersFormat.Format(pointsForNewPlayers);
        }

        MessageTemplateText messageText = Config.Texts.AcceptedFormat.Format(playersText, points);
        return messageText.SendAsync(this, chat);
    }

    internal Task OnNewGameAsync(Chat chat, User sender)
    {
        Contexts.Remove(sender.Id);
        MessageTemplateText message = Config.Texts.NewGame;
        return message.SendAsync(this, chat);
    }

    internal Task OnToggleLanguagesAsync(Chat chat, User sender)
    {
        Game.Data.Game? game = TryGetContext<Game.Data.Game>(sender.Id);
        if (game is null)
        {
            return OnNewGameAsync(chat, sender);
        }

        game.ToggleLanguages();

        MessageTemplateText message = game.IncludeEn ? Config.Texts.LangToggledToRuEn : Config.Texts.LangToggledToRu;
        return message.SendAsync(this, chat);
    }

    private async Task ReportNewGameAsync(Chat chat, Game.Data.Game game)
    {
        MessageTemplateText playersText =
            Config.Texts.PlayersFormat.Format(string.Join(PlayerSeparator, game.Players));
        MessageTemplateText startText = Config.Texts.NewGameFormat.Format(playersText);
        await startText.SendAsync(this, chat);

        await DrawActionAsync(chat, game);
    }

    private Game.Data.Game StartNewGame(List<PlayerListUpdate> updates)
    {
        if (_decksProvider is null)
        {
            throw new ArgumentNullException(nameof(_decksProvider));
        }

        PlayerRepository repository = new(updates);
        DistributedMatchmaker matchmaker = new(repository);

        List<IInteractionSubscriber> subscribers = new()
        {
            repository,
            matchmaker.InteractionRepository
        };

        return new Game.Data.Game(Config, _decksProvider, repository, matchmaker, subscribers);
    }

    private Task DrawActionAsync(Chat chat, Game.Data.Game game)
    {
        ActionInfo info = game.DrawAction();
        Game.Data.Cards.Action action = game.GetAction(info.ActionId);
        return ShowPartnersAsync(chat, info, action);
    }

    internal Task RevealCardAsync(Chat chat, int messageId, User sender, GameButtonInfo info)
    {
        Game.Data.Game? game = TryGetContext<Game.Data.Game>(sender.Id);
        if (game is null)
        {
            return OnNewGameAsync(chat, sender);
        }

        Turn turn;
        InlineKeyboardMarkup keyboard;
        switch (info)
        {
            case GameButtonInfoQuestion question:
                turn = game.DrawQuestion(question.Player);
                keyboard = CreateQuestionKeyboard(question.Player);
                break;
            case GameButtonInfoAction action:
                Game.Data.Cards.Action card = game.GetAction(action.ActionInfo.ActionId);

                string description;
                string descriptionEn;
                if (action.Tag == Config.ActionOptions[0].Tag)
                {
                    description = card.Description0;
                    descriptionEn = card.Description0En;
                }
                else if (action.Tag == Config.ActionOptions[1].Tag)
                {
                    description = card.Description1;
                    descriptionEn = card.Description1En;
                }
                else if (action.Tag == Config.ActionOptions[2].Tag)
                {
                    description = card.Description2;
                    descriptionEn = card.Description2En;
                }
                else
                {
                    throw new IndexOutOfRangeException("Unexpected tag");
                }

                turn = new Turn(Config.Texts, Config.ImagesFolder, action.Tag, description, descriptionEn,
                    action.ActionInfo, card.CompatablePartners, card.ImagePath);
                keyboard = CreateActionKeyboard(action.Tag, action.ActionInfo);
                break;
            default: throw new InvalidOperationException("Unexpected SelectOptionInfo");
        }

        MessageTemplate template = turn.GetMessage(game.IncludeEn);
        ParseMode? parseMode = template.MarkdownV2 ? ParseMode.MarkdownV2 : null;
        return EditMessageTextAsync(chat, messageId, template.EscapeIfNeeded(), parseMode, replyMarkup: keyboard);
    }

    internal Task CompleteCardAsync(Chat chat, User sender, GameButtonInfo info)
    {
        Game.Data.Game? game = TryGetContext<Game.Data.Game>(sender.Id);
        if (game is null)
        {
            return OnNewGameAsync(chat, sender);
        }

        switch (info)
        {
            case GameButtonInfoQuestion:
                game.RegisterQuestion();
                break;
            case GameButtonInfoAction action:
                OptionInfo optionInfo = Config.ActionOptions.Single(o => o.Tag == action.Tag);
                game.RegisterAction(action.ActionInfo, optionInfo.Points);
                break;
            default: throw new InvalidOperationException("Unexpected SelectOptionInfo");
        }
        return DrawActionAsync(chat, game);
    }

    private async Task ShowPartnersAsync(Chat chat, ActionInfo info, Game.Data.Cards.Action action)
    {
        string partnersText = "";
        if (info.Partners.Count > 0)
        {
            partnersText = Turn.GetPartnersPart(Config.Texts, info.Partners, action.CompatablePartners);
        }
        MessageTemplateText message = Config.Texts.TurnFormatShort.Format(info.Player, partnersText);
        message.KeyboardProvider = CreateCardKeyboard(info);
        await message.SendAsync(this, chat);
    }

    private InlineKeyboardMarkup CreateCardKeyboard(ActionInfo info)
    {
        List<List<InlineKeyboardButton>> keyboard = Config.ActionOptions
                                                          .AsEnumerable()
                                                          .Select(o => CreateActionButton(nameof(RevealCard), o.Tag, o.Tag, info))
                                                          .Select(CreateButtonRow)
                                                          .ToList();
        InlineKeyboardButton questionButton = new(Config.Texts.QuestionsTag)
        {
            CallbackData = nameof(RevealCard) + info.Player
        };
        keyboard.Insert(0, CreateButtonRow(questionButton));

        return new InlineKeyboardMarkup(keyboard);
    }

    private InlineKeyboardMarkup CreateActionKeyboard(string tag, ActionInfo info)
    {
        List<List<InlineKeyboardButton>> keyboard = new();
        InlineKeyboardButton questionButton = new(Config.Texts.QuestionsTag)
        {
            CallbackData = nameof(RevealCard) + info.Player
        };
        InlineKeyboardButton actionButton =
            CreateActionButton(nameof(CompleteCard), Config.Texts.ActionCompleted, tag, info);

        keyboard.Add(CreateButtonRow(questionButton));
        keyboard.Add(CreateButtonRow(actionButton));

        return new InlineKeyboardMarkup(keyboard);
    }

    private InlineKeyboardMarkup CreateQuestionKeyboard(string player)
    {
        List<List<InlineKeyboardButton>> keyboard = new()
        {
            CreateButtonRow(CreateQuestionButton(player))
        };

        return new InlineKeyboardMarkup(keyboard);
    }

    private static InlineKeyboardButton CreateActionButton(string operation, string caption, string tag,
        ActionInfo info)
    {
        return new InlineKeyboardButton(caption)
        {
            CallbackData =
                operation +
                $"{tag}{GameButtonInfo.Separator}" +
                $"{info.ActionId}{GameButtonInfo.Separator}" +
                $"{info.Player}{GameButtonInfo.Separator}" +
                string.Join(GameButtonInfo.Separator, info.Partners)
        };
    }

    private InlineKeyboardButton CreateQuestionButton(string player)
    {
        return new InlineKeyboardButton(Config.Texts.ActionCompleted)
        {
            CallbackData = nameof(CompleteCard) + player
        };
    }

    private static List<InlineKeyboardButton> CreateButtonRow(InlineKeyboardButton button)
    {
        return new List<InlineKeyboardButton> { button };
    }

    private Game.DecksProvider? _decksProvider;
    private readonly Sheet _actionsSheet;
    private readonly Sheet _questionsSheet;

    private const string PlayerSeparator = ", ";
}