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
using DaresGameBot.Game.Matchmaking.Compatibility;
using DaresGameBot.Helpers;
using DaresGameBot.Operations.Data.GameButtons;
using DaresGameBot.Operations.Data.PlayerListUpdates;
using JetBrains.Annotations;
using DaresGameBot.Context;
using DaresGameBot.Context.Meta;
using DaresGameBot.Game.Data;
using DaresGameBot.Save;

namespace DaresGameBot;

public sealed class Bot : BotWithSheets<Config, Texts, Context.Context, object, MetaContext, Data, CommandDataSimple>
{
    [Flags]
    internal enum AccessType
    {
        [UsedImplicitly]
        Default = 1,
        Player = 2,
        Admin = 4
    }

    public Bot(Config config) : base(config)
    {
        Operations.Add(new NewCommand(this));
        Operations.Add(new UpdateCommand(this));
        Operations.Add(new LangCommand(this));
        Operations.Add(new RatesCommand(this));
        Operations.Add(new RevealCard(this));
        Operations.Add(new UnrevealCard(this));
        Operations.Add(new CompleteCard(this));
        Operations.Add(new UpdatePlayers(this));
        Operations.Add(new ConfirmEnd(this));

        GoogleSheetsManager.Documents.Document document = DocumentsManager.GetOrAdd(Config.GoogleSheetId);

        _actionsSheet = document.GetOrAddSheet(Config.Texts.ActionsTitle);
        _questionsSheet = document.GetOrAddSheet(Config.Texts.QuestionsTitle);

        _adminChat = new Chat
        {
            Id = Config.AdminChatId,
            Type = ChatType.Private
        };
        _playerChat = new Chat
        {
            Id = Config.PlayerChatId,
            Type = ChatType.Private
        };
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken);

        await UpdateDecksAsync();

        SaveManager.Load();

        if (_game is null)
        {
            await UnpinPlayersAsync(cancellationToken);
        }
    }

    protected override KeyboardProvider GetDefaultKeyboardProvider(Chat _) => KeyboardProvider.Same;

    protected override void BeforeSave()
    {
        SaveManager.SaveData.GameData = _game?.Save();

        base.BeforeSave();
    }

    protected override void AfterLoad()
    {
        base.AfterLoad();

        MetaContext? meta = GetMetaContext();
        _game = SaveManager.SaveData.GameData is null ? null : Context.Game.Load(SaveManager.SaveData.GameData, meta);
    }

    protected override MetaContext? GetMetaContext()
    {
        if (_actions is null || _questions is null)
        {
            return null;
        }
        return new MetaContext(Config.ActionOptions, _actions, _questions, Config.Texts.ActionsTitle,
            Config.Texts.QuestionsTitle);
    }

    internal bool CanBeUpdated() => _game is null || (_game.CurrentState == Context.Game.State.ArrangementPurposed);

    internal async Task UpdatePlayersAsync(List<PlayerListUpdateData> updates)
    {
        if (_game is null)
        {
            List<string> toggled = updates.OfType<TogglePlayerData>().Select(t => t.Name).ToList();
            if (toggled.Any())
            {
                await ReportUnknownToggleAsync(toggled);
                return;
            }

            _game = StartNewGame(updates);

            await Config.Texts.NewGameStart.SendAsync(this, _adminChat);
            await Config.Texts.NewGameStart.SendAsync(this, _playerChat);

            SaveManager.SaveData.PlayersMessageId = null;
            await ReportAndPinPlayersAsync(_game);

            await DrawArrangementAsync(_game);
        }
        else
        {
            HashSet<string> toggled = new(updates.OfType<TogglePlayerData>().Select(t => t.Name));
            toggled.ExceptWith(_game.Players.AllNames);

            if (toggled.Any())
            {
                await ReportUnknownToggleAsync(toggled);
                return;
            }

            bool changed = _game.UpdatePlayers(updates);
            if (!changed)
            {
                await Config.Texts.NothingChanges.SendAsync(this, _adminChat);
                return;
            }

            await Config.Texts.Accepted.SendAsync(this, _adminChat);

            await DrawArrangementAsync(_game);

            await ReportAndPinPlayersAsync(_game);
        }

        SaveManager.Save();
    }

    internal Task OnEndGameRequestedAsync(ConfirmEndData.ActionAfterGameEnds after)
    {
        if (_game is null)
        {
            return DoRequestedActionAsync(after);
        }

        MessageTemplateText template = Config.Texts.EndGameWarning;
        template.KeyboardProvider = CreateEndGameConfirmationKeyboard(after);
        return template.SendAsync(this, _adminChat);
    }

    internal async Task OnEndGameConfirmedAsync(ConfirmEndData.ActionAfterGameEnds after)
    {
        if (_game is null)
        {
            return;
        }

        await ShowRatesAsync(_game);

        await EndGame();

        await DoRequestedActionAsync(after);
    }

    internal Task OnToggleLanguagesAsync()
    {
        if (_game is null)
        {
            return StartNewGameAsync();
        }

        SaveManager.SaveData.IncludeEn = !SaveManager.SaveData.IncludeEn;
        SaveManager.Save();

        MessageTemplateText message =
            SaveManager.SaveData.IncludeEn ? Config.Texts.LangToggledToRuEn : Config.Texts.LangToggledToRu;
        return message.SendAsync(this, _adminChat);
    }

    internal async Task RevealCardAsync(int messageId, RevealCardData revealData)
    {
        if (_game is null)
        {
            await StartNewGameAsync();
            return;
        }

        if ((messageId != SaveManager.SaveData.CardAdminMessageId)
            && (messageId != SaveManager.SaveData.CardPlayerMessageId))
        {
            return;
        }
        if (SaveManager.SaveData.CardAdminMessageId is null || SaveManager.SaveData.CardPlayerMessageId is null)
        {
            return;
        }

        MessageTemplate template;
        Turn turn;
        switch (revealData)
        {
            case RevealQuestionData q:
                ushort id = _game.DrawQuestion();
                turn = CreateQuestionTurn(_game, id);
                template = turn.GetMessage(SaveManager.SaveData.IncludeEn);
                await EditMessageAsync(_playerChat, template, SaveManager.SaveData.CardPlayerMessageId.Value);
                template.KeyboardProvider = CreateQuestionKeyboard(id, q.Arrangement);
                await EditMessageAsync(_adminChat, template, SaveManager.SaveData.CardAdminMessageId.Value);
                break;
            case RevealActionData a:
                ActionInfo actionInfo = _game.DrawAction(a.Arrangement, a.Tag);
                ActionData data = _game.GetActionData(actionInfo.Id);
                turn =
                    new Turn(Config.Texts, Config.ImagesFolder, data, _game.Players.Current, actionInfo.Arrangement);
                template = turn.GetMessage(SaveManager.SaveData.IncludeEn);
                bool includePartial = Config.ActionOptions[a.Tag].PartialPoints.HasValue;
                template.KeyboardProvider = CreateActionKeyboard(actionInfo, false, includePartial);
                await EditMessageAsync(_playerChat, template, SaveManager.SaveData.CardPlayerMessageId.Value);
                template.KeyboardProvider = CreateActionKeyboard(actionInfo, true, includePartial);
                await EditMessageAsync(_adminChat, template, SaveManager.SaveData.CardAdminMessageId.Value);
                break;
            default: throw new InvalidOperationException("Unexpected SelectOptionInfo");
        }

        SaveManager.Save();
    }

    internal async Task UnrevealCardAsync(int messageId, UnervealCardData data)
    {
        if (_game is null)
        {
            await StartNewGameAsync();
            return;
        }

        if ((messageId != SaveManager.SaveData.CardAdminMessageId)
            && (messageId != SaveManager.SaveData.CardPlayerMessageId))
        {
            return;
        }
        if (SaveManager.SaveData.CardAdminMessageId is null || SaveManager.SaveData.CardPlayerMessageId is null)
        {
            return;
        }

        MessageTemplateText? partnersText = null;
        if (data.Arrangement.Partners.Count > 0)
        {
            partnersText = Turn.GetPartnersPart(Config.Texts, data.Arrangement);
        }
        MessageTemplateText template = Config.Texts.TurnFormatShort.Format(_game.Players.Current, partnersText);
        template.KeyboardProvider = CreateArrangementKeyboard(data.Arrangement);

        await EditMessageAsync(_adminChat, template, SaveManager.SaveData.CardAdminMessageId.Value);
        await EditMessageAsync(_playerChat, template, SaveManager.SaveData.CardPlayerMessageId.Value);

        _game.ProcessCardUnrevealed();
    }

    internal Task CompleteCardAsync(CompleteCardData data)
    {
        if (_game is null)
        {
            return StartNewGameAsync();
        }

        switch (data)
        {
            case CompleteQuestionData q:
                _game.CompleteQuestion(q.Id, q.Arrangement);
                break;
            case CompleteActionData a:
                _game.CompleteAction(a.ActionInfo, a.CompletedFully);
                break;
            default: throw new InvalidOperationException("Unexpected SelectOptionInfo");
        }

        SaveManager.Save();

        SaveManager.SaveData.CardAdminMessageId = null;
        SaveManager.SaveData.CardPlayerMessageId = null;

        return DrawArrangementAsync(_game);
    }

    internal Task ShowRatesAsync() => _game is null ? StartNewGameAsync() : ShowRatesAsync(_game);

    private async Task EditMessageAsync(Chat chat, MessageTemplate template, int messageId)
    {
        switch (template)
        {
            case MessageTemplateText mtt:
                await mtt.EditMessageWithSelfAsync(this, chat, messageId);
                break;
            case MessageTemplateImage mti:
                await mti.EditMessageMediaWithSelfAsync(this, chat, messageId);
                await mti.EditMessageCaptionWithSelfAsync(this, chat, messageId);
                break;
            default: throw new InvalidOperationException();
        }
    }

    private Task DoRequestedActionAsync(ConfirmEndData.ActionAfterGameEnds after)
    {
        return after switch
        {
            ConfirmEndData.ActionAfterGameEnds.StartNewGame => StartNewGameAsync(),
            ConfirmEndData.ActionAfterGameEnds.UpdateCards  => UpdateDecksAsync(),
            _ => throw new ArgumentOutOfRangeException(nameof(after), after, null)
        };
    }

    private Task StartNewGameAsync() => Config.Texts.NewGame.SendAsync(this, _adminChat);

    private async Task UpdateDecksAsync()
    {
        _decksLoadErrors.Clear();
        _decksEquipment.Clear();
        await using (await StatusMessage.CreateAsync(this, _adminChat, Config.Texts.ReadingDecks, GetDecksLoadStatus))
        {
            List<ActionData> actionsList = await _actionsSheet.LoadAsync<ActionData>(Config.ActionsRange);

            HashSet<string> allTags = new();
            Dictionary<int, HashSet<string>> tags = new();
            foreach (ActionData data in actionsList)
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

                    ActionData data = actionsList.First(a => a.ArrangementType.GetHashCode() == hash);
                    string line = string.Format(Config.Texts.WrongArrangementFormat, data.Partners,
                        data.CompatablePartners, string.Join(Config.Texts.TagSeparator, tags[hash]));
                    _decksLoadErrors.Add(line);
                }

                if (_decksLoadErrors.Count == 0)
                {
                    _actions = GetIndexDictionary(actionsList);

                    List<CardData> questionsList = await _questionsSheet.LoadAsync<CardData>(Config.QuestionsRange);
                    _questions = GetIndexDictionary(questionsList);
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

    private static Dictionary<ushort, T> GetIndexDictionary<T>(IReadOnlyList<T> list)
    {
        Dictionary<ushort, T> dict = new();
        for (ushort i = 0; i < list.Count; ++i)
        {
            dict[i] = list[i];
        }
        return dict;
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

    private Task ReportUnknownToggleAsync(IEnumerable<string> names)
    {
        string text = string.Join(Config.Texts.DefaultSeparator, names);
        MessageTemplateText template = Config.Texts.UnknownToggleFormat.Format(text);
        return template.SendAsync(this, _adminChat);
    }

    private Context.Game StartNewGame(List<PlayerListUpdateData> updates)
    {
        if (_actions is null)
        {
            throw new ArgumentNullException(nameof(_actions));
        }
        Deck<ActionData> actionDeck = new(_actions);

        if (_questions is null)
        {
            throw new ArgumentNullException(nameof(_questions));
        }
        Deck<CardData> questionDeck = new(_questions);

        PlayersRepository repository = new();
        GameStats gameStats = new(Config.ActionOptions, _actions, repository);

        gameStats.UpdateList(updates);

        GroupCompatibility compatibility = new();
        DistributedMatchmaker matchmaker = new(repository, gameStats, compatibility);
        return new Context.Game(actionDeck, questionDeck, Config.Texts.ActionsTitle, Config.Texts.QuestionsTitle,
            repository, gameStats, matchmaker);
    }

    private async Task DrawArrangementAsync(Context.Game game)
    {
        Arrangement? arrangement = game.TryDrawArrangement();
        if (arrangement is not null)
        {
            await ShowArrangementAsync(game.Players.Current, arrangement);
            return;
        }

        await DrawAndSendQuestion(game);
    }

    private async Task DrawAndSendQuestion(Context.Game game)
    {
        ushort id = game.DrawQuestion();
        Turn turn = CreateQuestionTurn(game, id);
        MessageTemplate template = turn.GetMessage(SaveManager.SaveData.IncludeEn);
        Message message = await template.SendAsync(this, _playerChat);
        SaveManager.SaveData.CardPlayerMessageId = message.MessageId;

        template.KeyboardProvider = CreateQuestionKeyboard(id, null);
        message = await template.SendAsync(this, _adminChat);
        SaveManager.SaveData.CardAdminMessageId = message.MessageId;
    }

    private Turn CreateQuestionTurn(Context.Game game, ushort id)
    {
        CardData data = game.GetQuestionData(id);
        return new Turn(Config.Texts, Config.ImagesFolder, Config.Texts.QuestionsTag, data, game.Players.Current);
    }

    private async Task ShowArrangementAsync(string player, Arrangement arrangement)
    {
        MessageTemplateText? partnersText = null;
        if (arrangement.Partners.Count > 0)
        {
            partnersText = Turn.GetPartnersPart(Config.Texts, arrangement);
        }
        MessageTemplateText messageTemplate = Config.Texts.TurnFormatShort.Format(player, partnersText);
        messageTemplate.KeyboardProvider = CreateArrangementKeyboard(arrangement);

        Message message = await messageTemplate.SendAsync(this, _adminChat);
        SaveManager.SaveData.CardAdminMessageId = message.MessageId;

        message = await messageTemplate.SendAsync(this, _playerChat);
        SaveManager.SaveData.CardPlayerMessageId = message.MessageId;
    }

    private Task ShowRatesAsync(Context.Game game)
    {
        Dictionary<string, float> ratios = new();
        foreach (string player in game.Players.GetActiveNames())
        {
            float? rate = game.Stats.GetRatio(player);
            if (rate is not null)
            {
                ratios[player] = rate.Value;
            }
        }

        if (ratios.Count == 0)
        {
            return Config.Texts.NoRates.SendAsync(this, _adminChat);
        }

        float bestRate = ratios.Values.Max();

        List<MessageTemplateText> lines = new();

        // ReSharper disable once LoopCanBePartlyConvertedToQuery
        foreach (string player in ratios.Keys.OrderByDescending(p => ratios[p]))
        {
            uint points = game.Stats.GetPoints(player);
            uint propositions = game.Stats.GetPropositions(player);
            string rate = ratios[player].ToString("0.##");
            uint turns = game.Stats.GetTurns(player);

            MessageTemplateText line = Config.Texts.RateFormat.Format(player, points, propositions, rate, turns);
            if (Math.Abs(ratios[player] - bestRate) < float.Epsilon)
            {
                line = Config.Texts.BestRateFormat.Format(line);
            }
            line = Config.Texts.RateLineFormat.Format(line);
            lines.Add(line);
        }

        MessageTemplateText allLinesTemplate = MessageTemplateText.JoinTexts(lines);
        MessageTemplateText template = Config.Texts.RatesFormat.Format(allLinesTemplate);
        return template.SendAsync(this, _adminChat);
    }

    private async Task EndGame()
    {
        _game = null;
        SaveManager.SaveData.IncludeEn = false;
        SaveManager.SaveData.PlayersMessageId = null;
        SaveManager.SaveData.CardAdminMessageId = null;
        SaveManager.SaveData.CardPlayerMessageId = null;
        await UnpinPlayersAsync();
        SaveManager.Save();
    }

    private async Task ReportAndPinPlayersAsync(Context.Game game)
    {
        IEnumerable<string> players =
            game.Players.GetActiveNames().Select(p => string.Format(Config.Texts.PlayerFormat, p));
        MessageTemplateText messageText =
            Config.Texts.PlayersFormat.Format(string.Join(Config.Texts.PlayersSeparator, players));

        if (SaveManager.SaveData.PlayersMessageId is null)
        {
            Message message = await messageText.SendAsync(this, _adminChat);
            SaveManager.SaveData.PlayersMessageId = message.MessageId;
        }
        else
        {
            await messageText.EditMessageWithSelfAsync(this, _adminChat, SaveManager.SaveData.PlayersMessageId.Value);
        }

        await PinChatMessageAsync(_adminChat, SaveManager.SaveData.PlayersMessageId.Value);
    }

    private Task UnpinPlayersAsync(CancellationToken cancellationToken = default)
    {
        SaveManager.SaveData.PlayersMessageId = null;
        return UnpinAllChatMessagesAsync(_adminChat, cancellationToken);
    }

    private InlineKeyboardMarkup CreateArrangementKeyboard(Arrangement arrangement)
    {
        List<List<InlineKeyboardButton>> keyboard = new()
        {
            CreateOneButtonRow<RevealCard>(Config.Texts.QuestionsTag, GetString(arrangement))
        };

        keyboard.AddRange(Config.ActionOptions
                                .OrderBy(o => o.Value.Points)
                                .Select(o => CreateOneButtonRow<RevealCard>(o.Key, GetString(arrangement), o.Key)));

        return new InlineKeyboardMarkup(keyboard);
    }

    private InlineKeyboardMarkup CreateActionKeyboard(ActionInfo info, bool admin, bool includePartial)
    {
        string arrangementString = GetString(info.Arrangement);
        List<InlineKeyboardButton> unreveal =
            CreateOneButtonRow<UnrevealCard>(Config.Texts.Unreveal, arrangementString);
        List<InlineKeyboardButton> question =
            CreateOneButtonRow<RevealCard>(Config.Texts.QuestionsTag, arrangementString);

        List<InlineKeyboardButton> partial = CreateActionButtonRow(info, false);
        List<InlineKeyboardButton> full = CreateActionButtonRow(info, true);

        List<List<InlineKeyboardButton>> keyboard = new();
        if (admin)
        {
            keyboard.Add(unreveal);
            keyboard.Add(question);
            if (includePartial)
            {
                keyboard.Add(partial);
            }
            keyboard.Add(full);
        }
        else
        {
            keyboard.Add(question);
        }
        return new InlineKeyboardMarkup(keyboard);
    }

    private InlineKeyboardMarkup CreateQuestionKeyboard(ushort id, Arrangement? declinedArrangement)
    {
        List<List<InlineKeyboardButton>> keyboard = new();

        List<InlineKeyboardButton> complete;
        if (declinedArrangement is null)
        {
            complete = CreateOneButtonRow<CompleteCard>(Config.Texts.Completed, id);
            keyboard.Add(complete);
        }
        else
        {
            string arrangementString = GetString(declinedArrangement);

            List<InlineKeyboardButton> unreveal =
                CreateOneButtonRow<UnrevealCard>(Config.Texts.Unreveal, arrangementString);
            complete = CreateOneButtonRow<CompleteCard>(Config.Texts.Completed, arrangementString, id);

            keyboard.Add(unreveal);
            keyboard.Add(complete);
        }

        return new InlineKeyboardMarkup(keyboard);
    }

    private InlineKeyboardMarkup CreateEndGameConfirmationKeyboard(ConfirmEndData.ActionAfterGameEnds after)
    {
        List<List<InlineKeyboardButton>> keyboard = new()
        {
            CreateOneButtonRow<ConfirmEnd>(Config.Texts.Completed, after)
        };

        return new InlineKeyboardMarkup(keyboard);
    }

    private List<InlineKeyboardButton> CreateActionButtonRow(ActionInfo info, bool fully)
    {
        string caption = fully ? Config.Texts.Completed : Config.Texts.ActionCompletedPartially;
        return CreateOneButtonRow<CompleteCard>(caption, GetString(info.Arrangement), info.Id, fully);
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

    private static string GetString(Arrangement arrangement)
    {
        string partners = string.Join(GameButtonData.PartnersSeparator, arrangement.Partners);
        return $"{partners}{GameButtonData.FieldSeparator}{arrangement.CompatablePartners}";
    }

    private readonly Sheet _actionsSheet;
    private readonly Sheet _questionsSheet;
    private Dictionary<ushort, ActionData>? _actions;
    private Dictionary<ushort, CardData>? _questions;
    private Context.Game? _game;
    private readonly HashSet<string> _decksEquipment = new();
    private readonly List<string> _decksLoadErrors = new();
    private readonly Chat _adminChat;
    private readonly Chat _playerChat;
}