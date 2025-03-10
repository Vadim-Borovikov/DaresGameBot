using AbstractBot;
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
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using DaresGameBot.Game;
using DaresGameBot.Game.Matchmaking.Compatibility;
using DaresGameBot.Operations.Data.GameButtons;
using DaresGameBot.Operations.Data.PlayerListUpdates;
using JetBrains.Annotations;
using DaresGameBot.Game.Data;
using AbstractBot.Interfaces.Modules;
using AbstractBot.Models;
using AbstractBot.Models.MessageTemplates;
using AbstractBot.Models.Operations.Commands.Start;
using AbstractBot.Models.Operations.Commands;
using AbstractBot.Modules.TextProviders;
using AbstractBot.Modules;
using GryphonUtilities.Save;
using AbstractBot.Interfaces.Operations.Commands.Start;
using DaresGameBot.Utilities;
using DaresGameBot.Game.States;
using DaresGameBot.Game.States.Cores;
using DaresGameBot.Game.States.Data;
using AbstractBot.Modules.Context.Localization;
using AbstractBot.Modules.UserProviders;

namespace DaresGameBot;

public sealed class Bot : AbstractBot.Bot, IDisposable
{
    private readonly BotCore _core;
    private readonly Config _config;

    [Flags]
    internal enum AccessType
    {
        [UsedImplicitly]
        Default = 1,
        Player = 2,
        Admin = 4
    }

    public static async Task<Bot?> TryCreateAsync(Config config, CancellationToken cancellationToken)
    {
        BotCore? core = await BotCore.TryCreateAsync(config, cancellationToken);
        if (core is null)
        {
            return null;
        }

        core.UpdateSender.DefaultKeyboardProvider = KeyboardProvider.Same;

        SaveManager<BotState, BotData> saveManager = new(config.SavePath, core.Clock);

        Dictionary<long, UserState> userStates = new();

        Localization<Texts, UserState, LocalizationUserStateData> localization =
            new(config.AllTexts, config.DefaultLanguageCode, userStates);

        AccessBasedUserProvider accessBasedUserProvider = new(core.Accesses);
        CumulativeUserProvider userProvider = new(accessBasedUserProvider, localization);

        ICommands commands = new Commands(core.Client, core.Accesses, core.UpdateReceiver, localization, userProvider);

        Texts defaultTexts = localization.GetDefaultTexts();
        Greeter greeter = new(core.UpdateSender, defaultTexts.StartFormat);
        Start start = new(core.Accesses, core.UpdateSender, commands, localization, core.SelfUsername, greeter);

        Help help = new(core.Accesses, core.UpdateSender, core.UpdateReceiver, localization, core.SelfUsername);

        return new Bot(core, commands, start, help, config, saveManager, localization, userStates);
    }

    private Bot(BotCore core, ICommands commands, IStartCommand start, Help help, Config config,
        SaveManager<BotState, BotData> saveManager, ITextsProvider<Texts> textsProvider,
        Dictionary<long, UserState> userStates)
        : base(core, commands, start, help)
    {
        _core = core;
        _config = config;

        _sheetsManager = new Manager(_config);
        _saveManager = saveManager;
        _textsProvider = textsProvider;

        GoogleSheetsManager.Documents.Document document = _sheetsManager.GetOrAdd(_config.GoogleSheetId);

        Texts texts = textsProvider.GetDefaultTexts();

        _actionsSheet = document.GetOrAddSheet(texts.ActionsTitle);
        _questionsSheet = document.GetOrAddSheet(texts.QuestionsTitle);

        _adminChat = new Chat
        {
            Id = _config.AdminChatId,
            Type = ChatType.Private
        };
        _playerChat = new Chat
        {
            Id = _config.PlayerChatId,
            Type = ChatType.Private
        };

        BotStateCore stateCore = new(_config.ActionOptions, texts.ActionsTitle, texts.QuestionsTitle);
        _state = new BotState(stateCore, userStates);
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _core.UpdateReceiver.Operations.Add(new NewCommand(this, _textsProvider));
        _core.UpdateReceiver.Operations.Add(new RatesCommand(this, _textsProvider));
        _core.UpdateReceiver.Operations.Add(new UpdateCommand(this, _textsProvider));
        _core.UpdateReceiver.Operations.Add(new LangCommand(this, _textsProvider));
        _core.UpdateReceiver.Operations.Add(new UpdatePlayers(this, _textsProvider));
        _core.UpdateReceiver.Operations.Add(new RevealCard(this));
        _core.UpdateReceiver.Operations.Add(new UnrevealCard(this));
        _core.UpdateReceiver.Operations.Add(new CompleteCard(this));
        _core.UpdateReceiver.Operations.Add(new ConfirmEnd(this));

        await base.StartAsync(cancellationToken);

        await UpdateDecksAsync();

        _saveManager.LoadTo(_state);

        await Commands.UpdateForAll(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _saveManager.Save(_state);
        return base.StopAsync(cancellationToken);
    }

    public void Dispose()
    {
        _sheetsManager.Dispose();
        _core.Dispose();
    }

    internal bool CanBeUpdated() => _state.Game is null
                                    || (_state.Game.CurrentState == Game.States.Game.State.ArrangementPurposed);

    internal async Task UpdatePlayersAsync(List<PlayerListUpdateData> updates, User sender)
    {
        Texts texts = _textsProvider.GetTextsFor(sender);

        if (_state.Game is null)
        {
            List<string> toggled = updates.OfType<TogglePlayerData>().Select(t => t.Name).ToList();
            if (toggled.Any())
            {
                await ReportUnknownToggleAsync(toggled, sender);
                return;
            }

            _state.Game = StartNewGame(updates, sender);

            await texts.NewGameStart.SendAsync(_core.UpdateSender, _adminChat);
            await texts.NewGameStart.SendAsync(_core.UpdateSender, _playerChat);

            _state.PlayersMessageId = null;
            await ReportAndPinPlayersAsync(_state.Game, sender);

            await DrawArrangementAsync(_state.Game, sender);
        }
        else
        {
            HashSet<string> toggled = new(updates.OfType<TogglePlayerData>().Select(t => t.Name));
            toggled.ExceptWith(_state.Game.Players.AllNames);

            if (toggled.Any())
            {
                await ReportUnknownToggleAsync(toggled, sender);
                return;
            }

            bool changed = _state.Game.UpdatePlayers(updates);
            if (!changed)
            {
                await texts.NothingChanges.SendAsync(_core.UpdateSender, _adminChat);
                return;
            }

            await texts.Accepted.SendAsync(_core.UpdateSender, _adminChat);

            await DrawArrangementAsync(_state.Game, sender);

            await ReportAndPinPlayersAsync(_state.Game, sender);
        }

        _saveManager.Save(_state);
    }

    internal Task OnEndGameRequestedAsync(ConfirmEndData.ActionAfterGameEnds after, User sender)
    {
        if (_state.Game is null)
        {
            return DoRequestedActionAsync(after, sender);
        }

        Texts texts = _textsProvider.GetTextsFor(sender);
        MessageTemplateText template = texts.EndGameWarning;
        template.KeyboardProvider = CreateEndGameConfirmationKeyboard(after, sender);
        return template.SendAsync(_core.UpdateSender, _adminChat);
    }

    internal async Task OnEndGameConfirmedAsync(ConfirmEndData.ActionAfterGameEnds after, User sender)
    {
        if (_state.Game is null)
        {
            return;
        }

        await ShowRatesAsync(_state.Game, sender);

        await EndGame();

        await DoRequestedActionAsync(after, sender);
    }

    internal Task OnToggleLanguagesAsync(User sender)
    {
        if (!_state.UserStates.ContainsKey(sender.Id))
        {
            _state.UserStates[sender.Id] = new UserState();
        }

        _state.UserStates[sender.Id].LanguageCode =
            _state.UserStates[sender.Id].IncludeEn ? UserState.LocalizationRu : UserState.LocalizationRuEn;
        _saveManager.Save(_state);

        Texts texts = _textsProvider.GetTextsFor(sender);

        Commands.UpdateFor(sender);

        return texts.LangToggled.SendAsync(_core.UpdateSender, _adminChat);
    }

    internal async Task RevealCardAsync(int messageId, RevealCardData revealData, User sender)
    {
        if (_state.Game is null)
        {
            await StartNewGameAsync(sender);
            return;
        }

        if ((messageId != _state.CardAdminMessageId) && (messageId != _state.CardPlayerMessageId))
        {
            return;
        }
        if (_state.CardAdminMessageId is null || _state.CardPlayerMessageId is null)
        {
            return;
        }

        MessageTemplate template;
        Turn turn;
        switch (revealData)
        {
            case RevealQuestionData q:
                ushort id = _state.Game.DrawQuestion();
                turn = CreateQuestionTurn(_state.Game, sender, id);
                template = turn.GetMessage();
                await EditMessageAsync(_playerChat, template, _state.CardPlayerMessageId.Value);
                template.KeyboardProvider = CreateQuestionKeyboard(id, q.Arrangement, sender);
                await EditMessageAsync(_adminChat, template, _state.CardAdminMessageId.Value);
                break;
            case RevealActionData a:
                Texts texts = _textsProvider.GetTextsFor(sender);
                ActionInfo actionInfo = _state.Game.DrawAction(a.Arrangement, a.Tag);
                ActionData data = _state.Game.GetActionData(actionInfo.Id);
                bool includeEn = _state.UserStates.ContainsKey(sender.Id) && _state.UserStates[sender.Id].IncludeEn;
                turn = new Turn(texts, includeEn, _config.ImagesFolder, data, _state.Game.Players.Current,
                    actionInfo.Arrangement);
                template = turn.GetMessage();
                bool includePartial = _config.ActionOptions[a.Tag].PartialPoints.HasValue;
                template.KeyboardProvider = CreateActionKeyboard(actionInfo, false, includePartial, sender);
                await EditMessageAsync(_playerChat, template, _state.CardPlayerMessageId.Value);
                template.KeyboardProvider = CreateActionKeyboard(actionInfo, true, includePartial, sender);
                await EditMessageAsync(_adminChat, template, _state.CardAdminMessageId.Value);
                break;
            default: throw new InvalidOperationException("Unexpected SelectOptionInfo");
        }

        _saveManager.Save(_state);
    }

    internal async Task UnrevealCardAsync(int messageId, UnervealCardData data, User sender)
    {
        if (_state.Game is null)
        {
            await StartNewGameAsync(sender);
            return;
        }

        if ((messageId != _state.CardAdminMessageId) && (messageId != _state.CardPlayerMessageId))
        {
            return;
        }
        if (_state.CardAdminMessageId is null || _state.CardPlayerMessageId is null)
        {
            return;
        }

        Texts texts = _textsProvider.GetTextsFor(sender);
        MessageTemplateText? partnersText = null;
        if (data.Arrangement.Partners.Count > 0)
        {
            partnersText = Turn.GetPartnersPart(texts, data.Arrangement);
        }
        MessageTemplateText template = texts.TurnFormatShort.Format(_state.Game.Players.Current, partnersText);
        template.KeyboardProvider = CreateArrangementKeyboard(data.Arrangement, sender);

        await EditMessageAsync(_adminChat, template, _state.CardAdminMessageId.Value);
        await EditMessageAsync(_playerChat, template, _state.CardPlayerMessageId.Value);

        _state.Game.ProcessCardUnrevealed();

        _saveManager.Save(_state);
    }

    internal Task CompleteCardAsync(CompleteCardData data, User sender)
    {
        if (_state.Game is null)
        {
            return StartNewGameAsync(sender);
        }

        switch (data)
        {
            case CompleteQuestionData q:
                _state.Game.CompleteQuestion(q.Id, q.Arrangement);
                break;
            case CompleteActionData a:
                _state.Game.CompleteAction(a.ActionInfo, a.CompletedFully);
                break;
            default: throw new InvalidOperationException("Unexpected SelectOptionInfo");
        }

        _state.CardAdminMessageId = null;
        _state.CardPlayerMessageId = null;

        _saveManager.Save(_state);

        return DrawArrangementAsync(_state.Game, sender);
    }

    internal Task ShowRatesAsync(User sender)
    {
        return _state.Game is null ? StartNewGameAsync(sender) : ShowRatesAsync(_state.Game, sender);
    }

    private async Task EditMessageAsync(Chat chat, MessageTemplate template, int messageId)
    {
        switch (template)
        {
            case MessageTemplateText mtt:
                await mtt.EditMessageWithSelfAsync(_core.UpdateSender, chat, messageId);
                break;
            case MessageTemplateImage mti:
                await mti.EditMessageMediaWithSelfAsync(_core.UpdateSender, chat, messageId);
                await mti.EditMessageCaptionWithSelfAsync(_core.UpdateSender, chat, messageId);
                break;
            default: throw new InvalidOperationException();
        }
    }

    private Task DoRequestedActionAsync(ConfirmEndData.ActionAfterGameEnds after, User sender)
    {
        return after switch
        {
            ConfirmEndData.ActionAfterGameEnds.StartNewGame => StartNewGameAsync(sender),
            ConfirmEndData.ActionAfterGameEnds.UpdateCards  => UpdateDecksAsync(sender),
            _ => throw new ArgumentOutOfRangeException(nameof(after), after, null)
        };
    }

    private Task StartNewGameAsync(User sender)
    {
        Texts texts = _textsProvider.GetTextsFor(sender);
        return texts.NewGame.SendAsync(Core.UpdateSender, _adminChat);
    }

    private async Task UpdateDecksAsync(User? sender = null)
    {
        Texts texts = sender is null ? _textsProvider.GetDefaultTexts() : _textsProvider.GetTextsFor(sender);

        _decksLoadErrors.Clear();
        _decksEquipment.Clear();
        await using (await StatusMessage.CreateAsync(_core.UpdateSender, _adminChat, texts.ReadingDecks,
                         texts.StatusMessageStartFormat, texts.StatusMessageEndFormat,
                         () => GetDecksLoadStatus(sender)))
        {
            List<ActionData> actionsList = await _actionsSheet.LoadAsync<ActionData>(_config.ActionsRange);

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
                    foreach (string item in data.Equipment.Split(texts.EquipmentSeparatorSheet))
                    {
                        _decksEquipment.Add(item);
                    }
                }
            }

            List<string> optionsTags = _config.ActionOptions.Keys.ToList();
            if (allTags.SetEquals(optionsTags))
            {
                foreach (int hash in tags.Keys)
                {
                    if (allTags.SetEquals(tags[hash]))
                    {
                        continue;
                    }

                    ActionData data = actionsList.First(a => a.ArrangementType.GetHashCode() == hash);
                    string line = string.Format(texts.WrongArrangementFormat, data.Partners,
                        data.CompatablePartners, string.Join(texts.TagSeparator, tags[hash]));
                    _decksLoadErrors.Add(line);
                }

                if (_decksLoadErrors.Count == 0)
                {
                    Dictionary<ushort, ActionData> actions = GetIndexDictionary(actionsList);

                    List<CardData> questionsList = await _questionsSheet.LoadAsync<CardData>(_config.QuestionsRange);
                    Dictionary<ushort, CardData> questions = GetIndexDictionary(questionsList);

                    _state.Core.SheetInfo = new SheetInfo(actions, questions);
                }
            }
            else
            {
                string line = string.Format(texts.WrongTagsFormat,
                    string.Join(texts.TagSeparator, allTags),
                    string.Join(texts.TagSeparator, optionsTags));
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

    private MessageTemplateText GetDecksLoadStatus(User? sender)
    {
        Texts texts = sender is null ? _textsProvider.GetDefaultTexts() : _textsProvider.GetTextsFor(sender);

        if (_decksLoadErrors.Count == 0)
        {
            MessageTemplateText? equipmentPart = null;
            if (_decksEquipment.Count > 0)
            {
                string equipment =
                    TextHelper.FormatAndJoin(_decksEquipment, texts.EquipmentFormat, texts.EquipmentSeparatorMessage);
                equipmentPart = texts.EquipmentPrefixFormat.Format(equipment);
            }
            return texts.StatusMessageEndSuccessFormat.Format(equipmentPart);
        }

        string errors = TextHelper.FormatAndJoin(_decksLoadErrors, texts.ErrorFormat, texts.ErrorsSeparator);
        return texts.StatusMessageEndFailedFormat.Format(errors);
    }

    private Task ReportUnknownToggleAsync(IEnumerable<string> names, User sender)
    {
        Texts texts = _textsProvider.GetTextsFor(sender);
        string text = string.Join(texts.DefaultSeparator, names);
        MessageTemplateText template = texts.UnknownToggleFormat.Format(text);
        return template.SendAsync(_core.UpdateSender, _adminChat);
    }

    private Game.States.Game StartNewGame(List<PlayerListUpdateData> updates, User sender)
    {
        if (_state.Core.SheetInfo is null)
        {
            throw new ArgumentNullException(nameof(_state.Core.SheetInfo));
        }
        Deck<ActionData> actionDeck = new(_state.Core.SheetInfo.Actions);
        Deck<CardData> questionDeck = new(_state.Core.SheetInfo.Questions);

        PlayersRepository repository = new();
        GameStatsStateCore gameStatsStateCore =
            new(_state.Core.ActionOptions, _state.Core.SheetInfo.Actions, repository);
        GameStats gameStats = new(gameStatsStateCore);

        gameStats.UpdateList(updates);

        Texts texts = _textsProvider.GetTextsFor(sender);
        GroupCompatibility compatibility = new();
        DistributedMatchmaker matchmaker = new(repository, gameStats, compatibility);
        return new Game.States.Game(actionDeck, questionDeck, texts.ActionsTitle, texts.QuestionsTitle, repository,
            gameStats, matchmaker);
    }

    private async Task DrawArrangementAsync(Game.States.Game game, User sender)
    {
        Arrangement? arrangement = game.TryDrawArrangement();
        if (arrangement is not null)
        {
            await ShowArrangementAsync(game.Players.Current, arrangement, sender);
            return;
        }

        await DrawAndSendQuestion(game, sender);
    }

    private async Task DrawAndSendQuestion(Game.States.Game game, User sender)
    {
        ushort id = game.DrawQuestion();
        Turn turn = CreateQuestionTurn(game, sender, id);
        MessageTemplate template = turn.GetMessage();
        Message message = await template.SendAsync(_core.UpdateSender, _playerChat);
        _state.CardPlayerMessageId = message.MessageId;

        template.KeyboardProvider = CreateQuestionKeyboard(id, null, sender);
        message = await template.SendAsync(_core.UpdateSender, _adminChat);
        _state.CardAdminMessageId = message.MessageId;

        _saveManager.Save(_state);
    }

    private Turn CreateQuestionTurn(Game.States.Game game, User sender, ushort id)
    {
        Texts texts = _textsProvider.GetTextsFor(sender);
        CardData data = game.GetQuestionData(id);
        bool includeEn = _state.UserStates.ContainsKey(sender.Id) && _state.UserStates[sender.Id].IncludeEn;
        return new Turn(texts,  includeEn, _config.ImagesFolder, texts.QuestionsTag, data,             game.Players.Current);
    }

    private async Task ShowArrangementAsync(string player, Arrangement arrangement, User sender)
    {
        Texts texts = _textsProvider.GetTextsFor(sender);
        MessageTemplateText? partnersText = null;
        if (arrangement.Partners.Count > 0)
        {
            partnersText = Turn.GetPartnersPart(texts, arrangement);
        }
        MessageTemplateText messageTemplate = texts.TurnFormatShort.Format(player, partnersText);
        messageTemplate.KeyboardProvider = CreateArrangementKeyboard(arrangement, sender);

        Message message = await messageTemplate.SendAsync(_core.UpdateSender, _adminChat);
        _state.CardAdminMessageId = message.MessageId;

        message = await messageTemplate.SendAsync(_core.UpdateSender, _playerChat);
        _state.CardPlayerMessageId = message.MessageId;

        _saveManager.Save(_state);
    }

    private Task ShowRatesAsync(Game.States.Game game, User sender)
    {
        Texts texts = _textsProvider.GetTextsFor(sender);
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
            return texts.NoRates.SendAsync(_core.UpdateSender, _adminChat);
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

            MessageTemplateText line = texts.RateFormat.Format(player, points, propositions, rate, turns);
            if (Math.Abs(ratios[player] - bestRate) < float.Epsilon)
            {
                line = texts.BestRateFormat.Format(line);
            }
            line = texts.RateLineFormat.Format(line);
            lines.Add(line);
        }

        MessageTemplateText allLinesTemplate = MessageTemplateText.JoinTexts(lines);
        MessageTemplateText template = texts.RatesFormat.Format(allLinesTemplate);
        return template.SendAsync(_core.UpdateSender, _adminChat);
    }

    private async Task EndGame()
    {
        _state.Game = null;
        _state.CardAdminMessageId = null;
        _state.CardPlayerMessageId = null;
        _state.PlayersMessageId = null;
        await _core.UpdateSender.UnpinAllChatMessagesAsync(_adminChat);
        _saveManager.Save(_state);
    }

    private async Task ReportAndPinPlayersAsync(Game.States.Game game, User sender)
    {
        Texts texts = _textsProvider.GetTextsFor(sender);
        IEnumerable<string> players =
            game.Players.GetActiveNames().Select(p => string.Format(texts.PlayerFormat, p));
        MessageTemplateText messageText =
            texts.PlayersFormat.Format(string.Join(texts.PlayersSeparator, players));

        if (_state.PlayersMessageId is null)
        {
            Message message = await messageText.SendAsync(_core.UpdateSender, _adminChat);
            _state.PlayersMessageId = message.MessageId;
        }
        else
        {
            await messageText.EditMessageWithSelfAsync(_core.UpdateSender, _adminChat, _state.PlayersMessageId.Value);
        }

        await _core.UpdateSender.PinChatMessageAsync(_adminChat, _state.PlayersMessageId.Value);

        _saveManager.Save(_state);
    }

    private InlineKeyboardMarkup CreateArrangementKeyboard(Arrangement arrangement, User sender)
    {
        Texts texts = _textsProvider.GetTextsFor(sender);
        List<List<InlineKeyboardButton>> keyboard = new()
        {
            CreateOneButtonRow<RevealCard>(texts.QuestionsTag, GetString(arrangement))
        };

        keyboard.AddRange(_state.Core
                                .ActionOptions
                                .OrderBy(o => o.Value.Points)
                                .Select(o => CreateOneButtonRow<RevealCard>(o.Key, GetString(arrangement), o.Key)));

        return new InlineKeyboardMarkup(keyboard);
    }

    private InlineKeyboardMarkup CreateActionKeyboard(ActionInfo info, bool admin, bool includePartial, User sender)
    {
        Texts texts = _textsProvider.GetTextsFor(sender);
        string arrangementString = GetString(info.Arrangement);
        List<InlineKeyboardButton> unreveal =
            CreateOneButtonRow<UnrevealCard>(texts.Unreveal, arrangementString);
        List<InlineKeyboardButton> question =
            CreateOneButtonRow<RevealCard>(texts.QuestionsTag, arrangementString);

        List<InlineKeyboardButton> partial = CreateActionButtonRow(info, false, sender);
        List<InlineKeyboardButton> full = CreateActionButtonRow(info, true, sender);

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

    private InlineKeyboardMarkup CreateQuestionKeyboard(ushort id, Arrangement? declinedArrangement, User sender)
    {
        Texts texts = _textsProvider.GetTextsFor(sender);
        List<List<InlineKeyboardButton>> keyboard = new();

        List<InlineKeyboardButton> complete;
        if (declinedArrangement is null)
        {
            complete = CreateOneButtonRow<CompleteCard>(texts.Completed, id);
            keyboard.Add(complete);
        }
        else
        {
            string arrangementString = GetString(declinedArrangement);

            List<InlineKeyboardButton> unreveal =
                CreateOneButtonRow<UnrevealCard>(texts.Unreveal, arrangementString);
            complete = CreateOneButtonRow<CompleteCard>(texts.Completed, arrangementString, id);

            keyboard.Add(unreveal);
            keyboard.Add(complete);
        }

        return new InlineKeyboardMarkup(keyboard);
    }

    private InlineKeyboardMarkup CreateEndGameConfirmationKeyboard(ConfirmEndData.ActionAfterGameEnds after,
        User sender)
    {
        Texts texts = _textsProvider.GetTextsFor(sender);
        List<List<InlineKeyboardButton>> keyboard = new()
        {
            CreateOneButtonRow<ConfirmEnd>(texts.Completed, after)
        };

        return new InlineKeyboardMarkup(keyboard);
    }

    private List<InlineKeyboardButton> CreateActionButtonRow(ActionInfo info, bool fully, User sender)
    {
        Texts texts = _textsProvider.GetTextsFor(sender);
        string caption = fully ? texts.Completed : texts.ActionCompletedPartially;
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

    private readonly BotState _state;

    private readonly Sheet _actionsSheet;
    private readonly Sheet _questionsSheet;
    private readonly HashSet<string> _decksEquipment = new();
    private readonly List<string> _decksLoadErrors = new();
    private readonly Chat _adminChat;
    private readonly Chat _playerChat;
    private readonly Manager _sheetsManager;
    private readonly SaveManager<BotState, BotData> _saveManager;
    private readonly ITextsProvider<Texts> _textsProvider;
}