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

namespace DaresGameBot;

public sealed class Bot : AbstractBot.Bot, IDisposable
{
    [Flags]
    internal enum AccessType
    {
        [UsedImplicitly]
        Default = 1,
        Player = 2,
        Admin = 4
    }

    [PublicAPI]
    public readonly Cpu.Timer CpuTimer = new();

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

        Localization<Texts, UserState, UserStateData> localization =
            new(config.AllTexts, config.DefaultLanguageCode, userStates);

        ICommands commands =
            new Commands(core.Client, core.Accesses, core.UpdateReceiver, localization, userStates.Keys);


        Texts defaultTexts = localization.GetDefaultTexts();
        BotStateCore stateCore = new(config.ActionOptions, defaultTexts.ActionsTitle, defaultTexts.QuestionsTitle);
        BotState state = new(stateCore, userStates, config.AdminChatId, config.PlayerChatId);
        Greeter greeter = new(core.UpdateSender, localization);
        LocalizationUserRegistrator registrator = new(state, saveManager);
        Start start = new(core.Accesses, core.UpdateSender, commands, localization, core.SelfUsername, greeter,
            registrator);

        Help help = new(core.Accesses, core.UpdateSender, core.UpdateReceiver, localization, core.SelfUsername);

        return new Bot(core, commands, start, help, config, saveManager, state, localization);
    }

    private Bot(BotCore core, ICommands commands, IStartCommand start, Help help, Config config,
        SaveManager<BotState, BotData> saveManager, BotState state, ITextsProvider<Texts> textsProvider)
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

        _state = state;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await _core.Connection.StartAsync(cancellationToken);
        await _core.Logging.StartAsync(cancellationToken);

        await UpdateDecksAsync();

        _saveManager.LoadTo(_state);

        _core.UpdateReceiver.Operations.Add(new NewCommand(this, _textsProvider));
        _core.UpdateReceiver.Operations.Add(new RatesCommand(this, _textsProvider));
        _core.UpdateReceiver.Operations.Add(new UpdateCommand(this, _textsProvider));
        _core.UpdateReceiver.Operations.Add(new LangCommand(this, _textsProvider));
        _core.UpdateReceiver.Operations.Add(new UpdatePlayers(this, _textsProvider));
        _core.UpdateReceiver.Operations.Add(new RevealCard(this));
        _core.UpdateReceiver.Operations.Add(new UnrevealCard(this));
        _core.UpdateReceiver.Operations.Add(new CompleteCard(this));
        _core.UpdateReceiver.Operations.Add(new ConfirmEnd(this));

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

    internal async Task UpdatePlayersAsync(List<PlayerListUpdateData> updates)
    {
        Texts adminTexts = _textsProvider.GetTextsFor(_adminChat.Id);
        Texts playerTexts = _textsProvider.GetTextsFor(_playerChat.Id);

        if (_state.Game is null)
        {
            List<string> toggled = updates.OfType<TogglePlayerData>().Select(t => t.Name).ToList();
            if (toggled.Any())
            {
                await ReportUnknownToggleAsync(toggled);
                return;
            }

            _state.Game = StartNewGame(updates);

            await adminTexts.NewGameStart.SendAsync(_core.UpdateSender, _adminChat);
            await playerTexts.NewGameStart.SendAsync(_core.UpdateSender, _playerChat);

            _state.PlayersMessageId = null;
            await ReportAndPinPlayersAsync(_state.Game);

            await DrawArrangementAsync(_state.Game);
        }
        else
        {
            HashSet<string> toggled = new(updates.OfType<TogglePlayerData>().Select(t => t.Name));
            toggled.ExceptWith(_state.Game.Players.AllNames);

            if (toggled.Any())
            {
                await ReportUnknownToggleAsync(toggled);
                return;
            }

            bool changed = _state.Game.UpdatePlayers(updates);
            if (!changed)
            {
                await adminTexts.NothingChanges.SendAsync(_core.UpdateSender, _adminChat);
                return;
            }

            await adminTexts.Accepted.SendAsync(_core.UpdateSender, _adminChat);

            await DrawArrangementAsync(_state.Game);

            await ReportAndPinPlayersAsync(_state.Game);
        }

        _saveManager.Save(_state);
    }

    internal Task OnEndGameRequestedAsync(ConfirmEndData.ActionAfterGameEnds after)
    {
        if (_state.Game is null)
        {
            return DoRequestedActionAsync(after);
        }

        Texts adminTexts = _textsProvider.GetTextsFor(_adminChat.Id);
        MessageTemplateText template = adminTexts.EndGameWarning;
        template.KeyboardProvider = CreateEndGameConfirmationKeyboard(after);
        return template.SendAsync(_core.UpdateSender, _adminChat);
    }

    internal async Task OnEndGameConfirmedAsync(ConfirmEndData.ActionAfterGameEnds after)
    {
        if (_state.Game is null)
        {
            return;
        }

        await ShowRatesAsync(_state.Game);

        await EndGame();

        await DoRequestedActionAsync(after);
    }

    internal Task OnToggleLanguagesAsync(Chat chat, User sender)
    {
        if (!_state.UserStates.ContainsKey(sender.Id))
        {
            _state.UserStates[sender.Id] = new UserState();
        }

        _state.UserStates[sender.Id].LanguageCode =
            _state.UserStates[sender.Id].IncludeEn ? UserState.LocalizationRu : UserState.LocalizationRuEn;
        _saveManager.Save(_state);

        Texts texts = _textsProvider.GetTextsFor(sender.Id);

        Commands.UpdateFor(sender.Id);

        return texts.LangToggled.SendAsync(_core.UpdateSender, chat);
    }

    internal async Task RevealCardAsync(RevealCardData revealData)
    {
        if (_state.Game?.CurrentArrangement is null)
        {
            await StartNewGameAsync();
            return;
        }

        if (_state.AdminState?.CardMessageId is null || _state.PlayerState?.CardMessageId is null)
        {
            return;
        }

        if (string.IsNullOrEmpty(revealData.Tag))
        {
            _state.Game.DrawQuestion();
            await RevealQuestionAsync(_state.Game, _state.PlayerState.CardMessageId.Value, _playerChat);
            await RevealQuestionAsync(_state.Game, _state.AdminState.CardMessageId.Value, _adminChat);
        }
        else
        {
            _state.Game.DrawAction(revealData.Tag);
            ActionData data = _state.Game.GetActionData();
            bool includePartial = _config.ActionOptions[revealData.Tag].PartialPoints.HasValue;
            await RevealActionAsync(_state.Game.Players.Current, _state.Game.CurrentArrangement, data, includePartial,
                _state.PlayerState.CardMessageId.Value, _playerChat);
            await RevealActionAsync(_state.Game.Players.Current, _state.Game.CurrentArrangement, data, includePartial,
                _state.AdminState.CardMessageId.Value, _adminChat);
        }

        _saveManager.Save(_state);
    }

    internal async Task UnrevealCardAsync()
    {
        if (_state.Game?.CurrentArrangement is null)
        {
            await StartNewGameAsync();
            return;
        }

        if (_state.AdminState?.CardMessageId is null || _state.PlayerState?.CardMessageId is null)
        {
            return;
        }

        await UnrevealCardAsync(_state.Game, _state.Game.CurrentArrangement, _playerChat,
            _state.PlayerState.CardMessageId.Value);
        await UnrevealCardAsync(_state.Game, _state.Game.CurrentArrangement, _adminChat,
            _state.AdminState.CardMessageId.Value);

        _state.Game.ProcessCardUnrevealed();
        _saveManager.Save(_state);
    }

    internal async Task CompleteCardAsync(CompleteCardData data)
    {
        if (_state.Game is null)
        {
            await StartNewGameAsync();
            return;
        }

        if (_state.AdminState?.CardMessageId is null || _state.PlayerState?.CardMessageId is null)
        {
            return;
        }

        bool fully = true;
        if (data.Fully is null)
        {
            _state.Game.CompleteQuestion();
        }
        else
        {
            fully = data.Fully.Value;
            _state.Game.CompleteAction(fully);
        }

        await ShowCardAsCompletedAsync(_playerChat, _state.PlayerState.CardMessageId.Value, data.Template, fully);
        await ShowCardAsCompletedAsync(_adminChat, _state.AdminState.CardMessageId.Value, data.Template, fully);

        _state.ResetUserMessageId(_playerChat.Id);
        _state.ResetUserMessageId(_adminChat.Id);

        _saveManager.Save(_state);

        await DrawArrangementAsync(_state.Game);
    }

    internal Task ShowRatesAsync() => _state.Game is null ? StartNewGameAsync() : ShowRatesAsync(_state.Game);

    private Task RevealQuestionAsync(Game.States.Game game, int cardMessageId, Chat chat)
    {
        Turn turn = CreateQuestionTurn(game, chat.Id);
        MessageTemplate template = turn.GetMessage();
        InlineKeyboardMarkup? keyboard = CreateQuestionKeyboard(true, chat.Id);
        if (keyboard is not null)
        {
            template.KeyboardProvider = keyboard;
        }
        return EditMessageAsync(chat, template, cardMessageId);
    }

    private Task RevealActionAsync(string player, Arrangement arrangement, ActionData data, bool includePartial,
        int cardMessageId, Chat chat)
    {
        Texts texts = _textsProvider.GetTextsFor(chat.Id);
        bool includeEn = _state.UserStates.ContainsKey(chat.Id) && _state.UserStates[chat.Id].IncludeEn;
        Turn turn = new(texts, includeEn, _config.ImagesFolder, data, player, arrangement);
        MessageTemplate template = turn.GetMessage();
        template.KeyboardProvider = CreateActionKeyboard(includePartial, chat.Id);
        return EditMessageAsync(chat, template, cardMessageId);
    }

    private Task UnrevealCardAsync(Game.States.Game game, Arrangement arrangement, Chat chat, int cardMessageId)
    {
        Texts texts = _textsProvider.GetTextsFor(chat.Id);
        MessageTemplateText? partnersText = null;
        if (arrangement.Partners.Count > 0)
        {
            partnersText = Turn.GetPartnersPart(texts, arrangement);
        }
        MessageTemplateText template = texts.TurnFormatShort.Format(game.Players.Current, partnersText);
        template.KeyboardProvider = CreateArrangementKeyboard(chat.Id);
        return EditMessageAsync(chat, template, cardMessageId);
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

    private Task DoRequestedActionAsync(ConfirmEndData.ActionAfterGameEnds after)
    {
        return after switch
        {
            ConfirmEndData.ActionAfterGameEnds.StartNewGame => StartNewGameAsync(),
            ConfirmEndData.ActionAfterGameEnds.UpdateCards  => UpdateDecksAsync(),
            _ => throw new ArgumentOutOfRangeException(nameof(after), after, null)
        };
    }

    private Task StartNewGameAsync()
    {
        Texts texts = _textsProvider.GetTextsFor(_adminChat.Id);
        return texts.NewGame.SendAsync(Core.UpdateSender, _adminChat);
    }

    private async Task UpdateDecksAsync()
    {
        Texts texts = _textsProvider.GetTextsFor(_adminChat.Id);

        _decksLoadErrors.Clear();
        _decksEquipment.Clear();
        await using (await StatusMessage.CreateAsync(_core.UpdateSender, _adminChat, texts.ReadingDecks,
                         texts.StatusMessageStartFormat, texts.StatusMessageEndFormat, GetDecksLoadStatus))
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

    private MessageTemplateText GetDecksLoadStatus()
    {
        Texts texts = _textsProvider.GetTextsFor(_adminChat.Id);

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

    private Task ReportUnknownToggleAsync(IEnumerable<string> names)
    {
        Texts texts = _textsProvider.GetTextsFor(_adminChat.Id);
        string text = string.Join(texts.DefaultSeparator, names);
        MessageTemplateText template = texts.UnknownToggleFormat.Format(text);
        return template.SendAsync(_core.UpdateSender, _adminChat);
    }

    private Game.States.Game StartNewGame(List<PlayerListUpdateData> updates)
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

        Texts texts = _textsProvider.GetDefaultTexts();
        GroupCompatibility compatibility = new();
        DistributedMatchmaker matchmaker = new(repository, gameStats, compatibility);
        return new Game.States.Game(actionDeck, questionDeck, texts.ActionsTitle, texts.QuestionsTitle, repository,
            gameStats, matchmaker);
    }

    private async Task DrawArrangementAsync(Game.States.Game game)
    {
        game.DrawArrangement();
        if (game.CurrentArrangement is null)
        {
            await DrawAndSendQuestionAsync(game);
            return;
        }

        await ShowArrangementAsync(game.Players.Current, game.CurrentArrangement, _adminChat);
        await ShowArrangementAsync(game.Players.Current, game.CurrentArrangement, _playerChat);

        _saveManager.Save(_state);
    }

    private async Task DrawAndSendQuestionAsync(Game.States.Game game)
    {
        game.DrawQuestion();

        await SendQuestionAsync(game, _adminChat);
        await SendQuestionAsync(game, _playerChat);

        _saveManager.Save(_state);
    }

    private async Task SendQuestionAsync(Game.States.Game game, Chat chat)
    {
        Turn turn = CreateQuestionTurn(game, chat.Id);
        MessageTemplate template = turn.GetMessage();
        InlineKeyboardMarkup? keyboard = CreateQuestionKeyboard(false, chat.Id);
        if (keyboard is not null)
        {
            template.KeyboardProvider = keyboard;
        }
        Message message = await template.SendAsync(_core.UpdateSender, chat);
        _state.SetUserMessageId(chat.Id, message.MessageId);
    }

    private Turn CreateQuestionTurn(Game.States.Game game, long userId)
    {
        Texts texts = _textsProvider.GetTextsFor(userId);
        bool includeEn = _state.UserStates.ContainsKey(userId) && _state.UserStates[userId].IncludeEn;
        CardData data = game.GetQuestionData();
        List<string> players = new() { game.Players.Current };
        if (game.CurrentArrangement is not null)
        {
            players.AddRange(game.CurrentArrangement.Partners);
        }
        return new Turn(texts,  includeEn, _config.ImagesFolder, texts.QuestionsTag, data, players);
    }

    private async Task ShowArrangementAsync(string player, Arrangement arrangement, Chat chat)
    {
        Texts texts = _textsProvider.GetTextsFor(chat.Id);
        MessageTemplateText? partnersText = null;
        if (arrangement.Partners.Count > 0)
        {
            partnersText = Turn.GetPartnersPart(texts, arrangement);
        }
        MessageTemplateText messageTemplate = texts.TurnFormatShort.Format(player, partnersText);
        messageTemplate.KeyboardProvider = CreateArrangementKeyboard(chat.Id);

        Message message = await messageTemplate.SendAsync(_core.UpdateSender, chat);
        _state.SetUserMessageId(chat.Id, message.MessageId);

        _saveManager.Save(_state);
    }

    private Task ShowCardAsCompletedAsync(Chat chat, int messageId, MessageTemplateText original, bool fully)
    {
        Texts texts = _textsProvider.GetTextsFor(chat.Id);

        string completedPart = fully ? texts.Completed : texts.ActionCompletedPartially;
        MessageTemplateText template = texts.CompletedCardFormat.Format(original, completedPart);

        return EditMessageAsync(chat, template, messageId);
    }

    private Task ShowRatesAsync(Game.States.Game game)
    {
        Texts texts = _textsProvider.GetTextsFor(_adminChat.Id);
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
        foreach (UserState userState in _state.UserStates.Values)
        {
            userState.CardMessageId = null;
        }
        _state.PlayersMessageId = null;
        await _core.UpdateSender.UnpinAllChatMessagesAsync(_adminChat);
        _saveManager.Save(_state);
    }

    private async Task ReportAndPinPlayersAsync(Game.States.Game game)
    {
        Texts texts = _textsProvider.GetTextsFor(_adminChat.Id);

        List<(string Name, bool Active)> players = game.Players.GetAllNamesWithStatus().ToList();
        List<string> playerLines = new();
        for (int i = 0; i < players.Count; ++i)
        {
            string format = players[i].Active ? texts.PlayerFormatActive : texts.PlayerFormatInactive;
            string line = string.Format(format, i, players[i].Name);
            playerLines.Add(line);
        }

        MessageTemplateText messageText = texts.PlayersFormat.Format(string.Join(texts.PlayersSeparator, playerLines));

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

    private InlineKeyboardMarkup CreateArrangementKeyboard(long userId)
    {
        Texts texts = _textsProvider.GetTextsFor(userId);
        List<List<InlineKeyboardButton>> keyboard = new()
        {
            CreateOneButtonRow<RevealCard>(texts.QuestionsTag)
        };

        keyboard.AddRange(_state.Core
                                .ActionOptions
                                .OrderBy(o => o.Value.Points)
                                .Select(o => CreateOneButtonRow<RevealCard>(o.Key, o.Key)));

        return new InlineKeyboardMarkup(keyboard);
    }

    private InlineKeyboardMarkup CreateActionKeyboard(bool includePartial, long userId)
    {
        Texts texts = _textsProvider.GetTextsFor(userId);
        List<InlineKeyboardButton> unreveal = CreateOneButtonRow<UnrevealCard>(texts.Unreveal);
        List<InlineKeyboardButton> question = CreateOneButtonRow<RevealCard>(texts.QuestionsTag);

        List<InlineKeyboardButton> partial = CreateActionButtonRow(false, userId);
        List<InlineKeyboardButton> full = CreateActionButtonRow(true, userId);

        List<List<InlineKeyboardButton>> keyboard = new();
        if (userId == _adminChat.Id)
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

    private InlineKeyboardMarkup? CreateQuestionKeyboard(bool arrangementWasDeclined, long userId)
    {
        if (userId != _adminChat.Id)
        {
            return null;
        }

        Texts texts = _textsProvider.GetTextsFor(userId);
        List<List<InlineKeyboardButton>> keyboard = new();

        List<InlineKeyboardButton> complete;
        if (arrangementWasDeclined)
        {
            List<InlineKeyboardButton> unreveal = CreateOneButtonRow<UnrevealCard>(texts.Unreveal);
            complete = CreateOneButtonRow<CompleteCard>(texts.Completed);

            keyboard.Add(unreveal);
            keyboard.Add(complete);
        }
        else
        {
            complete = CreateOneButtonRow<CompleteCard>(texts.Completed);
            keyboard.Add(complete);
        }

        return new InlineKeyboardMarkup(keyboard);
    }

    private InlineKeyboardMarkup CreateEndGameConfirmationKeyboard(ConfirmEndData.ActionAfterGameEnds after)
    {
        Texts texts = _textsProvider.GetTextsFor(_adminChat.Id);
        List<List<InlineKeyboardButton>> keyboard = new()
        {
            CreateOneButtonRow<ConfirmEnd>(texts.Completed, after)
        };

        return new InlineKeyboardMarkup(keyboard);
    }

    private List<InlineKeyboardButton> CreateActionButtonRow(bool fully, long userId)
    {
        Texts texts = _textsProvider.GetTextsFor(userId);
        string caption = fully ? texts.Completed : texts.ActionCompletedPartially;
        return CreateOneButtonRow<CompleteCard>(caption, fully);
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
            CallbackData = typeof(TData).Name + string.Join(TextHelper.FieldSeparator, fields)
        };
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

    private readonly BotCore _core;
    private readonly Config _config;
}