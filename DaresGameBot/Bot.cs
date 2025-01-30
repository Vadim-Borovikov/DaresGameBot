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
using DaresGameBot.Game;
using DaresGameBot.Game.Data;
using DaresGameBot.Game.Decks;
using DaresGameBot.Game.Players;
using DaresGameBot.Helpers;
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

    protected override KeyboardProvider GetDefaultKeyboardProvider(Chat _) => KeyboardProvider.Same;

    internal async Task UpdateDecksAsync(Chat chat)
    {
        await UnpinAllChatMessagesAsync(chat);
        Contexts.Remove(chat.Id);

        _decksLoadErrors.Clear();
        _decksEquipment.Clear();
        await using (await StatusMessage.CreateAsync(this, chat, Config.Texts.ReadingDecks, GetDecksLoadStatus))
        {
            List<ActionData> actionDatas = await _actionsSheet.LoadAsync<ActionData>(Config.ActionsRange);

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

                if (data.Equipment is not null && (data.Equipment.Length > 0))
                {
                    foreach (string item in data.Equipment.Split(Config.Texts.EquipmentSeparatorSheet))
                    {
                        _decksEquipment.Add(item);
                    }
                }
            }

            List<string> optionsTags = Config.ActionOptions.Keys.ToList();
            if (allTags.SetEquals(optionsTags))
            {
                foreach (int hash in tags.Keys)
                {
                    if (allTags.SetEquals(tags[hash]))
                    {
                        continue;
                    }

                    ActionData data = actionDatas.First(a => a.ArrangementType.GetHashCode() == hash);
                    string line = string.Format(Config.Texts.WrongArrangementFormat, data.Partners,
                        data.CompatablePartners, string.Join(Config.Texts.TagSeparator, tags[hash]));
                    _decksLoadErrors.Add(line);
                }

                if (_decksLoadErrors.Count == 0)
                {
                    List<CardData> questionDatas = await _questionsSheet.LoadAsync<CardData>(Config.QuestionsRange);
                    _decksProvider = new DecksProvider(actionDatas, questionDatas);
                }
            }
            else
            {
                string line = string.Format(Config.Texts.WrongTagsFormat,
                    string.Join(Config.Texts.TagSeparator, allTags),
                    string.Join(Config.Texts.TagSeparator, optionsTags));
                _decksLoadErrors.Add(line);
            }
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

            await Config.Texts.NewGameStart.SendAsync(this, chat);
            Message pin = await ReportPlayersAsync(chat, game, true);

            await PinChatMessageAsync(chat, pin.MessageId);

            await DrawActionOrQuestionAsync(chat, game);
        }
        else
        {
            bool changed = game.UpdatePlayers(updates);
            if (!changed)
            {
                return;
            }

            await Config.Texts.Accepted.SendAsync(this, chat);

            await DrawActionOrQuestionAsync(chat, game);

            await ReportPlayersAsync(chat, game, true);
        }
    }

    internal async Task OnNewGameAsync(Chat chat, User sender)
    {
        await UnpinAllChatMessagesAsync(chat);
        Contexts.Remove(sender.Id);
        MessageTemplateText message = Config.Texts.NewGame;
        await message.SendAsync(this, chat);
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

    private MessageTemplateText GetDecksLoadStatus()
    {
        if (_decksLoadErrors.Count == 0)
        {
            MessageTemplateText? equipmentPart = null;
            if (_decksEquipment.Count > 0)
            {
                string equipment = TextHelper.FormatAndJoin(_decksEquipment, Config.Texts.EquipmentFormat,
                    Config.Texts.EquipmentSeparatorMessage);
                equipmentPart = Config.Texts.EquipmentPrefixFormat.Format(equipment);
            }
            return Config.Texts.StatusMessageEndSuccessFormat.Format(equipmentPart);
        }

        string errors =
            TextHelper.FormatAndJoin(_decksLoadErrors, Config.Texts.ErrorFormat, Config.Texts.ErrorsSeparator);
        return Config.Texts.StatusMessageEndFailedFormat.Format(errors);
    }

    private async Task<Message> ReportPlayersAsync(Chat chat, Game.Game game, bool includePoints)
    {
        IEnumerable<string> players = GetPlayerList(game, includePoints);
        MessageTemplateText messageText =
            Config.Texts.PlayersFormat.Format(string.Join(Config.Texts.PlayersSeparator, players));

        if (game.PlayersMessage is null)
        {
            game.PlayersMessage = await messageText.SendAsync(this, chat);
        }
        else
        {
            await EditMessageTextAsync(chat, game.PlayersMessage.MessageId, messageText.EscapeIfNeeded());
        }

        game.PlayersMessageShowsPoints = includePoints;
        return game.PlayersMessage;
    }

    private IEnumerable<string> GetPlayerList(Game.Game game, bool includePoints)
    {
        return game.GetPlayers().Select(p => GetPlayerLine(p, game, includePoints));
    }

    private string GetPlayerLine(string name, Game.Game game, bool includePoints)
    {
        string? pointsPostfix = null;
        if (includePoints)
        {
            pointsPostfix = string.Format(Config.Texts.PlayerFormatPointsPostfix, game.GetPoints(name));
        }
        return string.Format(Config.Texts.PlayerFormat, name, pointsPostfix);
    }

    private Game.Game StartNewGame(List<PlayerListUpdateData> updates)
    {
        if (_decksProvider is null)
        {
            throw new ArgumentNullException(nameof(_decksProvider));
        }

        Repository repository = new(updates);
        PointsManager pointsManager = new(Config.ActionOptions, repository);
        DistributedMatchmaker matchmaker = new(repository, pointsManager);
        return new Game.Game(Config, _decksProvider, repository, pointsManager, matchmaker);
    }

    private async Task DrawActionOrQuestionAsync(Chat chat, Game.Game game)
    {
        Arrangement? arrangement = game.TryDrawArrangement();
        if (arrangement is null)
        {
            MessageTemplate template = CreateQuestionTemplate(game);
            await template.SendAsync(this, chat);
            return;
        }
        await ShowPartnersAsync(chat, game.CurrentPlayer, arrangement);
        game.OnActionPurposed(arrangement);
    }

    internal async Task RevealCardAsync(Chat chat, int messageId, User sender, GameButtonData buttonData)
    {
        Game.Game? game = TryGetContext<Game.Game>(sender.Id);
        if (game is null)
        {
            await OnNewGameAsync(chat, sender);
            return;
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
                Turn turn = new(Config.Texts, Config.ImagesFolder, data, game.CurrentPlayer, actionInfo.Arrangement);
                template = turn.GetMessage(game.IncludeEn);
                bool includePartial = Config.ActionOptions[a.Tag].PartialPoints.HasValue;
                template.KeyboardProvider = CreateActionKeyboard(actionInfo, includePartial);
                break;
            default: throw new InvalidOperationException("Unexpected SelectOptionInfo");
        }

        ParseMode parseMode = template.MarkdownV2 ? ParseMode.MarkdownV2 : ParseMode.None;
        if (template.KeyboardProvider is null)
        {
            throw new InvalidOperationException();
        }

        await EditMessageTextAsync(chat, messageId, template.EscapeIfNeeded(), parseMode,
            replyMarkup: template.KeyboardProvider.Keyboard as InlineKeyboardMarkup);
        if (game.PlayersMessageShowsPoints)
        {
            await ReportPlayersAsync(chat, game, false);
        }
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
                game.OnActionCompleted(a.ActionInfo, a.CompletedFully);
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
                                                          .OrderBy(o => o.Value.Points)
                                                          .Select(o => CreateActionButton(nameof(RevealCard), o.Key, o.Key, info))
                                                          .Select(CreateButtonRow)
                                                          .ToList();
        InlineKeyboardButton questionButton = new(Config.Texts.QuestionsTag)
        {
            CallbackData = nameof(RevealCard)
        };
        keyboard.Insert(0, CreateButtonRow(questionButton));

        return new InlineKeyboardMarkup(keyboard);
    }

    private InlineKeyboardMarkup CreateActionKeyboard(ActionInfo info, bool includePartial)
    {
        List<List<InlineKeyboardButton>> keyboard = new();
        InlineKeyboardButton questionButton = new(Config.Texts.QuestionsTag)
        {
            CallbackData = nameof(RevealCard)
        };
        keyboard.Add(CreateButtonRow(questionButton));

        if (includePartial)
        {
            InlineKeyboardButton actionButtonPartial = CreateActionButton(nameof(CompleteCard), false, info);
            keyboard.Add(CreateButtonRow(actionButtonPartial));
        }

        InlineKeyboardButton actionButton = CreateActionButton(nameof(CompleteCard), true, info);
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

    private InlineKeyboardButton CreateActionButton(string operation, bool completedFully, ActionInfo info)
    {
        string caption = completedFully ? Config.Texts.ActionCompleted : Config.Texts.ActionCompletedPartially;
        return new InlineKeyboardButton(caption)
        {
            CallbackData = operation
                           + $"{CreateArrangementButtonData(info.Arrangement)}{GameButtonData.FieldSeparator}"
                           + $"{info.Id}{GameButtonData.FieldSeparator}"
                           + completedFully
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
    private readonly HashSet<string> _decksEquipment = new();
    private readonly List<string> _decksLoadErrors = new();
}