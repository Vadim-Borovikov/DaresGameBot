using AbstractBot;
using AbstractBot.Bots;
using AbstractBot.Configs.MessageTemplates;
using DaresGameBot.Configs;
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
using DaresGameBot.Game.Matchmaking.Interactions;
using DaresGameBot.Game;
using DaresGameBot.Game.Data;
using DaresGameBot.Game.Decks;
using DaresGameBot.Game.Players;
using DaresGameBot.Operations.Data.GameButtons;
using DaresGameBot.Operations.Data.PlayerListUpdates;

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
        Contexts.Remove(chat.Id);

        MessageTemplateText? errors = null;

        await using (await StatusMessage.CreateAsync(this, chat, Config.Texts.ReadingDecks))
        {
            List<ActionData> actionDatas =
                await _actionsSheet.LoadAsync<ActionData>(Config.ActionsRange);

            HashSet<string> allTags = new();
            Dictionary<int, HashSet<string>> tags = new();
            foreach (ActionData data in actionDatas)
            {
                data.ArrangementType = new ArrangementType(data.Partners, data.CompatablePartners);

                int hash = data.ArrangementType.GetHashCode();
                allTags.Add(data.Tag);

                if (!tags.ContainsKey(hash))
                {
                    tags[hash] = new HashSet<string>();
                }
                tags[hash].Add(data.Tag);
            }

            List<string> optionsTags = Config.ActionOptions.Select(o => o.Tag).ToList();
            if (allTags.SetEquals(optionsTags))
            {
                List<string> errorLines = new();
                foreach (int hash in tags.Keys)
                {
                    if (allTags.SetEquals(tags[hash]))
                    {
                        continue;
                    }

                    ActionData data = actionDatas.First(a => a.ArrangementType.GetHashCode() == hash);
                    string line = string.Format(Config.Texts.WrongArrangementLineFormat, data.Partners,
                        data.CompatablePartners, string.Join("", tags[hash]));
                    errorLines.Add(line);
                }

                if (errorLines.Count == 0)
                {
                    List<QuestionData> questionDatas =
                        await _questionsSheet.LoadAsync<QuestionData>(Config.QuestionsRange);
                    _decksProvider = new DecksProvider(actionDatas, questionDatas);
                }
                else
                {
                    errors = Config.Texts.WrongArrangementFormat.Format(string.Join(Environment.NewLine, errorLines));
                }
            }
            else
            {
                errors = Config.Texts.WrongTagsFormat.Format(string.Join("", allTags), string.Join("", optionsTags));
            }
        }

        if (errors is not null)
        {
            await errors.SendAsync(this, chat);
        }
    }

    internal bool CanBeUpdated(User sender)
    {
        Game.Game? game = TryGetContext<Game.Game>(sender.Id);
        return game is null || (game.CurrentState == Game.Game.State.ArrangementPresented);
    }

    internal async Task UpdatePlayersAsync(Chat chat, User sender, List<PlayerListUpdateData> updates)
    {
        Game.Game? game = TryGetContext<Game.Game>(sender.Id);
        if (game is null)
        {
            game = StartNewGame(updates);
            Contexts[sender.Id] = game;

            await ReportPlayersAsync(chat, game, Config.Texts.NewGameFormat);
        }
        else
        {
            game.UpdatePlayers(updates);

            await ReportPlayersAsync(chat, game, Config.Texts.AcceptedFormat);
        }

        await DrawActionOrQuestionAsync(chat, game);
    }

    internal Task OnNewGameAsync(Chat chat, User sender)
    {
        Contexts.Remove(sender.Id);
        MessageTemplateText message = Config.Texts.NewGame;
        return message.SendAsync(this, chat);
    }

    internal Task OnToggleLanguagesAsync(Chat chat, User sender)
    {
        Game.Game? game = TryGetContext<Game.Game>(sender.Id);
        if (game is null)
        {
            return OnNewGameAsync(chat, sender);
        }

        game.ToggleLanguages();

        MessageTemplateText message = game.IncludeEn ? Config.Texts.LangToggledToRuEn : Config.Texts.LangToggledToRu;
        return message.SendAsync(this, chat);
    }

    private async Task ReportPlayersAsync(Chat chat, Game.Game game, MessageTemplate template)
    {
        MessageTemplateText playersText =
            Config.Texts.PlayersFormat.Format(string.Join(PlayerSeparator, GetPlayerList(game)));

        MessageTemplate messageText = template.Format(playersText);
        Message message = await messageText.SendAsync(this, chat);
        await UnpinAllChatMessagesAsync(chat);
        await PinChatMessageAsync(chat, message.MessageId);
    }

    private IEnumerable<string> GetPlayerList(Game.Game game)
    {
        return game.GetPlayers().Select(p => string.Format(Config.Texts.PlayerFormat, p, game.GetPoints(p)));
    }

    private Game.Game StartNewGame(List<PlayerListUpdateData> updates)
    {
        if (_decksProvider is null)
        {
            throw new ArgumentNullException(nameof(_decksProvider));
        }

        Repository repository = new(updates);
        DistributedMatchmaker matchmaker = new(repository);

        List<IInteractionSubscriber> subscribers = new()
        {
            repository,
            matchmaker.InteractionRepository
        };

        return new Game.Game(Config, _decksProvider, repository, matchmaker, subscribers);
    }

    private Task DrawActionOrQuestionAsync(Chat chat, Game.Game game)
    {
        Arrangement? arrangement = game.TryDrawArrangement();
        if (arrangement is null)
        {
            MessageTemplate template = CreateQuestionTemplate(game);
            return template.SendAsync(this, chat);
        }
        return ShowPartnersAsync(chat, game.CurrentPlayer, arrangement);
    }

    internal Task RevealCardAsync(Chat chat, int messageId, User sender, GameButtonData buttonData)
    {
        Game.Game? game = TryGetContext<Game.Game>(sender.Id);
        if (game is null)
        {
            return OnNewGameAsync(chat, sender);
        }

        MessageTemplate template;
        switch (buttonData)
        {
            case GameButtonQuestionData:
                template = CreateQuestionTemplate(game);
                break;
            case GameButtonArrangementData a:
                ActionInfo actionInfo = game.DrawAction(a.Arrangement, a.Tag);
                ActionData data = game.GetActionData(actionInfo.Id);
                Turn turn = new(Config.Texts, Config.ImagesFolder, data.Tag, data.Description, data.DescriptionEn,
                    game.CurrentPlayer, actionInfo.Arrangement, data.ImagePath);
                template = turn.GetMessage(game.IncludeEn);
                template.KeyboardProvider = CreateActionKeyboard(actionInfo);
                break;
            default: throw new InvalidOperationException("Unexpected SelectOptionInfo");
        }

        ParseMode? parseMode = template.MarkdownV2 ? ParseMode.MarkdownV2 : null;
        if (template.KeyboardProvider is null)
        {
            throw new InvalidOperationException();
        }
        return EditMessageTextAsync(chat, messageId, template.EscapeIfNeeded(), parseMode,
            replyMarkup: template.KeyboardProvider.Keyboard as InlineKeyboardMarkup);
    }

    private MessageTemplate CreateQuestionTemplate(Game.Game game)
    {
        Turn turn = game.DrawQuestion();
        MessageTemplate template = turn.GetMessage(game.IncludeEn);
        template.KeyboardProvider = CreateQuestionKeyboard();
        return template;
    }

    internal Task CompleteCardAsync(Chat chat, User sender, GameButtonData data)
    {
        Game.Game? game = TryGetContext<Game.Game>(sender.Id);
        if (game is null)
        {
            return OnNewGameAsync(chat, sender);
        }

        switch (data)
        {
            case GameButtonQuestionData:
                game.RegisterQuestion();
                break;
            case GameButtonActionData a:
                ActionData actionData = game.GetActionData(a.ActionInfo.Id);
                Option option = Config.ActionOptions.Single(o => o.Tag == actionData.Tag);
                game.RegisterAction(a.ActionInfo, option.Points);
                break;
            default: throw new InvalidOperationException("Unexpected SelectOptionInfo");
        }
        return DrawActionOrQuestionAsync(chat, game);
    }

    private Task ShowPartnersAsync(Chat chat, string player, Arrangement arrangement)
    {
        MessageTemplateText? partnersText = null;
        if (arrangement.Partners.Count > 0)
        {
            partnersText = Turn.GetPartnersPart(Config.Texts, arrangement);
        }
        MessageTemplateText message = Config.Texts.TurnFormatShort.Format(player, partnersText);
        message.KeyboardProvider = CreateCardKeyboard(arrangement);
        return message.SendAsync(this, chat);
    }

    private InlineKeyboardMarkup CreateCardKeyboard(Arrangement info)
    {
        List<List<InlineKeyboardButton>> keyboard = Config.ActionOptions
                                                          .AsEnumerable()
                                                          .Select(o => CreateActionButton(nameof(RevealCard), o.Tag, o.Tag, info))
                                                          .Select(CreateButtonRow)
                                                          .ToList();
        InlineKeyboardButton questionButton = new(Config.Texts.QuestionsTag)
        {
            CallbackData = nameof(RevealCard)
        };
        keyboard.Insert(0, CreateButtonRow(questionButton));

        return new InlineKeyboardMarkup(keyboard);
    }

    private InlineKeyboardMarkup CreateActionKeyboard(ActionInfo info)
    {
        List<List<InlineKeyboardButton>> keyboard = new();
        InlineKeyboardButton questionButton = new(Config.Texts.QuestionsTag)
        {
            CallbackData = nameof(RevealCard)
        };
        InlineKeyboardButton actionButton =
            CreateActionButton(nameof(CompleteCard), Config.Texts.ActionCompleted, info);

        keyboard.Add(CreateButtonRow(questionButton));
        keyboard.Add(CreateButtonRow(actionButton));

        return new InlineKeyboardMarkup(keyboard);
    }

    private InlineKeyboardMarkup CreateQuestionKeyboard()
    {
        List<List<InlineKeyboardButton>> keyboard = new()
        {
            CreateButtonRow(CreateQuestionButton())
        };

        return new InlineKeyboardMarkup(keyboard);
    }

    private static InlineKeyboardButton CreateActionButton(string operation, string caption, ActionInfo info)
    {
        return new InlineKeyboardButton(caption)
        {
            CallbackData = operation
                           + $"{CreateArrangementButtonData(info.Arrangement)}{GameButtonData.FieldSeparator}"
                           + info.Id
        };
    }

    private static InlineKeyboardButton CreateActionButton(string operation, string caption, string tag,
        Arrangement info)
    {
        return new InlineKeyboardButton(caption)
        {
            CallbackData = operation
                           + $"{CreateArrangementButtonData(info)}{GameButtonData.FieldSeparator}"
                           + tag
        };
    }

    private static string CreateArrangementButtonData(Arrangement arrangement)
    {
        return $"{string.Join(GameButtonData.ListSeparator, arrangement.Partners)}{GameButtonData.FieldSeparator}"
               + arrangement.CompatablePartners;
    }

    private InlineKeyboardButton CreateQuestionButton()
    {
        return new InlineKeyboardButton(Config.Texts.ActionCompleted)
        {
            CallbackData = nameof(CompleteCard)
        };
    }

    private static List<InlineKeyboardButton> CreateButtonRow(InlineKeyboardButton button)
    {
        return new List<InlineKeyboardButton> { button };
    }

    private DecksProvider? _decksProvider;
    private readonly Sheet _actionsSheet;
    private readonly Sheet _questionsSheet;

    private static readonly string PlayerSeparator = Environment.NewLine;
}