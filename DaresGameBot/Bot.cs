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
using DaresGameBot.Game.Matchmaking.Compatibility;
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
                    _actionDeck = new Deck<ActionData>(actionDatas);

                    List<CardData> questionDatas = await _questionsSheet.LoadAsync<CardData>(Config.QuestionsRange);
                    _questionsDeck = new Deck<CardData>(questionDatas);
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
        return game is null || (game.CurrentState == Game.Game.State.ArrangementPurposed);
    }

    internal async Task UpdatePlayersAsync(Chat chat, User sender, List<PlayerListUpdateData> updates)
    {
        Game.Game? game = TryGetContext<Game.Game>(sender.Id);
        if (game is null)
        {
            List<string> toggled = updates.OfType<TogglePlayerData>().Select(t => t.Name).ToList();
            if (toggled.Any())
            {
                await ReportUnknownToggleAsync(chat,  toggled);
                return;
            }

            game = StartNewGame(updates);
            Contexts[sender.Id] = game;

            await Config.Texts.NewGameStart.SendAsync(this, chat);
            Message pin = await ReportPlayersAsync(chat, game, true);

            await PinChatMessageAsync(chat, pin.MessageId);

            await DrawArrangementAsync(chat, game);
        }
        else
        {
            HashSet<string> toggled = new(updates.OfType<TogglePlayerData>().Select(t => t.Name));
            toggled.ExceptWith(game.Players.AllNames);

            if (toggled.Any())
            {
                await ReportUnknownToggleAsync(chat,  toggled);
                return;
            }

            bool changed = game.UpdatePlayers(updates);
            if (!changed)
            {
                await Config.Texts.NothingChanges.SendAsync(this, chat);
                return;
            }

            await Config.Texts.Accepted.SendAsync(this, chat);

            await DrawArrangementAsync(chat, game);

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

        _includeEn = !_includeEn;

        MessageTemplateText message = _includeEn ? Config.Texts.LangToggledToRuEn : Config.Texts.LangToggledToRu;
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

    private Task ReportUnknownToggleAsync(Chat chat, IEnumerable<string> names)
    {
        string text = string.Join(Config.Texts.UnknownToggleNamesSeparator, names);
        MessageTemplateText template = Config.Texts.UnknownToggleFormat.Format(text);
        return template.SendAsync(this, chat);
    }

    private async Task<Message> ReportPlayersAsync(Chat chat, Game.Game game, bool includePoints)
    {
        IEnumerable<string> players = game.Players.GetActiveNames().Select(p => GetPlayerLine(p, game, includePoints));
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

    private string GetPlayerLine(string name, Game.Game game, bool includePoints)
    {
        string? pointsPostfix = null;
        if (includePoints)
        {
            pointsPostfix = string.Format(Config.Texts.PlayerFormatPointsPostfix, game.Stats.Points[name]);
        }
        return string.Format(Config.Texts.PlayerFormat, name, pointsPostfix);
    }

    private Game.Game StartNewGame(List<PlayerListUpdateData> updates)
    {
        if (_actionDeck is null)
        {
            throw new ArgumentNullException(nameof(_actionDeck));
        }

        if (_questionsDeck is null)
        {
            throw new ArgumentNullException(nameof(_questionsDeck));
        }

        Repository repository = new();
        GameStats gameStats = new(Config.ActionOptions, _actionDeck, repository);

        gameStats.UpdateList(updates);

        GroupCompatibility compatibility = new();
        DistributedMatchmaker matchmaker = new(repository, gameStats, compatibility);
        return new Game.Game(_actionDeck, _questionsDeck, repository, gameStats, matchmaker);
    }

    private async Task DrawArrangementAsync(Chat chat, Game.Game game)
    {
        Arrangement? arrangement = game.TryDrawArrangement();
        if (arrangement is null)
        {
            MessageTemplate template = DrawQuestionAndCreateTemplate(game);
            await template.SendAsync(this, chat);
            return;
        }
        await ShowArrangementAsync(chat, game.Players.Current, arrangement);
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
            case GameButtonRevealQuestionData:
                template = DrawQuestionAndCreateTemplate(game);
                break;
            case GameButtonArrangementData a:
                ActionInfo actionInfo = game.DrawAction(a.Arrangement, a.Tag);
                ActionData data = game.GetActionData(actionInfo.Id);
                Turn turn = new(Config.Texts, Config.ImagesFolder, data, game.Players.Current, actionInfo.Arrangement);
                template = turn.GetMessage(_includeEn);
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

    private MessageTemplate DrawQuestionAndCreateTemplate(Game.Game game)
    {
        ushort id = game.DrawQuestion();
        CardData questionData = game.GetQuestionData(id);
        Turn turn =
            new(Config.Texts, Config.ImagesFolder, Config.Texts.QuestionsTag, questionData, game.Players.Current);
        MessageTemplate template = turn.GetMessage(_includeEn);
        template.KeyboardProvider = CreateQuestionKeyboard(id);
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
            case GameButtonCompleteQuestionData q:
                game.CompleteQuestion(q.Id);
                break;
            case GameButtonActionData a:
                game.CompleteAction(a.ActionInfo, a.CompletedFully);
                break;
            default: throw new InvalidOperationException("Unexpected SelectOptionInfo");
        }
        return DrawArrangementAsync(chat, game);
    }

    private Task ShowArrangementAsync(Chat chat, string player, Arrangement arrangement)
    {
        MessageTemplateText? partnersText = null;
        if (arrangement.Partners.Count > 0)
        {
            partnersText = Turn.GetPartnersPart(Config.Texts, arrangement);
        }
        MessageTemplateText message = Config.Texts.TurnFormatShort.Format(player, partnersText);
        message.KeyboardProvider = CreateArrangementKeyboard(arrangement);
        return message.SendAsync(this, chat);
    }

    private InlineKeyboardMarkup CreateArrangementKeyboard(Arrangement arrangement)
    {
        List<List<InlineKeyboardButton>> keyboard = new()
        {
            CreateOneButtonRow<RevealCard>(Config.Texts.QuestionsTag)
        };

        keyboard.AddRange(Config.ActionOptions
                                .OrderBy(o => o.Value.Points)
                                .Select(o => CreateOneButtonRow<RevealCard>(o.Key,
                                    string.Join(GameButtonData.ListSeparator, arrangement.Partners),
                                    arrangement.CompatablePartners, o.Key)));

        return new InlineKeyboardMarkup(keyboard);
    }

    private InlineKeyboardMarkup CreateActionKeyboard(ActionInfo info, bool includePartial)
    {
        List<List<InlineKeyboardButton>> keyboard = new()
        {
            CreateOneButtonRow<RevealCard>(Config.Texts.QuestionsTag),
            CreateActionButtonRow(info, true)
        };

        if (includePartial)
        {
            List<InlineKeyboardButton> partialRow = CreateActionButtonRow(info, false);
            keyboard.Insert(1, partialRow);
        }

        return new InlineKeyboardMarkup(keyboard);
    }

    private InlineKeyboardMarkup CreateQuestionKeyboard(ushort id)
    {
        List<List<InlineKeyboardButton>> keyboard = new()
        {
            CreateOneButtonRow<CompleteCard>(Config.Texts.Completed, id)
        };

        return new InlineKeyboardMarkup(keyboard);
    }

    private List<InlineKeyboardButton> CreateActionButtonRow(ActionInfo info, bool fully)
    {
        string caption = fully ? Config.Texts.Completed : Config.Texts.ActionCompletedPartially;
        return CreateOneButtonRow<CompleteCard>(caption,
            string.Join(GameButtonData.ListSeparator, info.Arrangement.Partners), info.Arrangement.CompatablePartners,
            info.Id, fully);
    }

    private static List<InlineKeyboardButton> CreateOneButtonRow<TData>(string caption, params object[] args)
    {
        return new List<InlineKeyboardButton>
        {
            CreateButton<TData>(caption, args)
        };
    }

    private static InlineKeyboardButton CreateButton<TData>(string caption, params object[] fields)
    {
        return new InlineKeyboardButton(caption)
        {
            CallbackData = typeof(TData).Name + string.Join(GameButtonData.FieldSeparator, fields)
        };
    }

    private readonly Sheet _actionsSheet;
    private readonly Sheet _questionsSheet;
    private Deck<ActionData>? _actionDeck;
    private Deck<CardData>? _questionsDeck;
    private readonly HashSet<string> _decksEquipment = new();
    private readonly List<string> _decksLoadErrors = new();
    private bool _includeEn;
}