using AbstractBot;
using AbstractBot.Interfaces.Modules;
using AbstractBot.Interfaces.Operations.Commands.Start;
using AbstractBot.Models;
using AbstractBot.Models.MessageTemplates;
using AbstractBot.Models.Operations.Commands;
using AbstractBot.Models.Operations.Commands.Start;
using AbstractBot.Modules;
using AbstractBot.Modules.TextProviders;
using DaresGameBot.Configs;
using DaresGameBot.Game;
using DaresGameBot.Game.Data;
using DaresGameBot.Game.Matchmaking;
using DaresGameBot.Game.Matchmaking.Compatibility;
using DaresGameBot.Game.States;
using DaresGameBot.Game.States.Cores;
using DaresGameBot.Game.States.Data;
using DaresGameBot.Operations;
using DaresGameBot.Operations.Commands;
using DaresGameBot.Operations.Data.GameButtons;
using DaresGameBot.Operations.Data.PlayerListUpdates;
using DaresGameBot.Utilities;
using GoogleSheetsManager.Documents;
using GryphonUtilities.Save;
using JetBrains.Annotations;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QRCoder;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

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
        BotStateCore stateCore = new(config.ActionOptions, config.QuestionPoints, config.ActionsTitle,
            config.QuestionsTitle, defaultTexts.PlayerFillNamePrefix);
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

        _actionsSheet = document.GetOrAddSheet(config.ActionsTitle);
        _questionsSheet = document.GetOrAddSheet(config.QuestionsTitle);

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
        _core.UpdateReceiver.Operations.Add(new ShowImagesCommand(this, _textsProvider));
        _core.UpdateReceiver.Operations.Add(new UpdatePlayers(this, _textsProvider));
        _core.UpdateReceiver.Operations.Add(new TogglePlayersMessageState(this));
        _core.UpdateReceiver.Operations.Add(new TogglePlayer(this));
        _core.UpdateReceiver.Operations.Add(new SelectPlayer(this));
        _core.UpdateReceiver.Operations.Add(new MovePlayerDown(this));
        _core.UpdateReceiver.Operations.Add(new MovePlayerToBottom(this));
        _core.UpdateReceiver.Operations.Add(new RearrangePlayer(this));
        _core.UpdateReceiver.Operations.Add(new RevealCard(this));
        _core.UpdateReceiver.Operations.Add(new UnrevealCard(this));
        _core.UpdateReceiver.Operations.Add(new DeleteCard(this));
        _core.UpdateReceiver.Operations.Add(new CompleteCard(this));
        _core.UpdateReceiver.Operations.Add(new ConfirmEnd(this));
        _core.UpdateReceiver.Operations.Add(new DrawCard(this));

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

    internal async Task UpdatePlayersAsync(List<AddOrUpdatePlayerData> updates)
    {
        Texts adminTexts = _textsProvider.GetTextsFor(_adminChat.Id);
        Texts playerTexts = _textsProvider.GetTextsFor(_playerChat.Id);

        if (_state.Game is null)
        {
            _state.Game = StartNewGame(updates);

            await adminTexts.NewGameStart.SendAsync(_core.UpdateSender, _adminChat);
            await playerTexts.NewGameStart.SendAsync(_core.UpdateSender, _playerChat);

            _state.PlayersMessageId = null;
            _state.CurrentPlayersMessageState = PlayersMessageState.Type.NewRearrangement;
        }
        else if (_state.Game.CurrentState == Game.States.Game.State.CardRevealed)
        {
            await adminTexts.Refuse.SendAsync(Core.UpdateSender, _adminChat);
            return;
        }
        else
        {
            bool changed = _state.Game.UpdatePlayers(updates, _textsProvider.GetDefaultTexts().UpdatePlayerSeparator);
            if (!changed)
            {
                await adminTexts.NothingChanges.SendAsync(_core.UpdateSender, _adminChat);
                return;
            }

            if (!_state.Game.IsCurrentArrangementValid())
            {
                await DeleteCardMessagesAsync();
            }

            await adminTexts.Accepted.SendAsync(_core.UpdateSender, _adminChat);
        }

        await ReportAndPinPlayersAsync(_state.Game);
    }

    internal async Task TogglePlayerAsync(string id)
    {
        if (_state.Game is null)
        {
            await StartNewGameAsync();
            return;
        }

        if (_state.Game.CurrentState == Game.States.Game.State.CardRevealed)
        {
            Texts adminTexts = _textsProvider.GetTextsFor(_adminChat.Id);
            await adminTexts.Refuse.SendAsync(Core.UpdateSender, _adminChat);
            return;
        }

        bool toggled = _state.Game.Players.Toggle(id);
        if (!toggled)
        {
            return;
        }

        if (!_state.Game.IsCurrentArrangementValid())
        {
            await DeleteCardMessagesAsync();
        }

        await ReportAndPinPlayersAsync(_state.Game);
    }

    internal async Task RearrangePlayerAsync(string id)
    {
        if (_state.Game is null)
        {
            await StartNewGameAsync();
            return;
        }

        bool newRearrangement = _state.CurrentPlayersMessageState == PlayersMessageState.Type.NewRearrangement;
        if (newRearrangement)
        {
            _state.Game.Players.DeactivateAll();
            _state.CurrentPlayersMessageState = PlayersMessageState.Type.Rearrangement;
        }

        bool toggled = _state.Game.Players.Toggle(id);
        if (!toggled)
        {
            return;
        }

        if (newRearrangement)
        {
            _state.Game.Players.Select(id);
        }

        if (_state.Game.Players.IsActive(id))
        {
            _state.Game.Players.MoveDown(id, true, true);
        }

        await ReportAndPinPlayersAsync(_state.Game);
    }

    internal async Task SelectPlayerAsync(string id)
    {
        if (_state.Game is null)
        {
            await StartNewGameAsync();
            return;
        }

        if (_state.Game.CurrentState == Game.States.Game.State.CardRevealed)
        {
            Texts adminTexts = _textsProvider.GetTextsFor(_adminChat.Id);
            await adminTexts.Refuse.SendAsync(Core.UpdateSender, _adminChat);
            return;
        }

        bool selected = _state.Game.Players.Select(id);
        if (!selected)
        {
            return;
        }

        await DeleteCardMessagesAsync();
        await DrawArrangementAsync(_state.Game);
        await ReportAndPinPlayersAsync(_state.Game);
    }

    internal async Task MovePlayerDownAsync(string name, bool toBottom = false)
    {
        if (_state.Game is null)
        {
            await StartNewGameAsync();
            return;
        }

        if (_state.Game.CurrentState == Game.States.Game.State.CardRevealed)
        {
            Texts adminTexts = _textsProvider.GetTextsFor(_adminChat.Id);
            await adminTexts.Refuse.SendAsync(Core.UpdateSender, _adminChat);
            return;
        }

        bool moved =
            _state.Game.Players.MoveDown(name, toBottom, _state.Game.CurrentState != Game.States.Game.State.Fresh);
        if (!moved)
        {
            return;
        }

        if (!_state.Game.IsCurrentArrangementValid())
        {
            await DeleteCardMessagesAsync();
        }

        await ReportAndPinPlayersAsync(_state.Game);
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
        if (_state.Game?.Players.Current is null)
        {
            await StartNewGameAsync();
            return;
        }

        if (_state.AdminState?.CardMessageId is null || _state.PlayerState?.CardMessageId is null)
        {
            return;
        }

        bool hasPartners = _state.Game.CurrentArrangement?.Partners.Count > 0;
        string? image = hasPartners ? GetArrangementImage(_state.Game.CurrentArrangement, revealData) : null;

        if (hasPartners && !string.IsNullOrEmpty(revealData.Tag))
        {
            if (_state.Game.CurrentArrangement is null || image is null)
            {
                throw new InvalidOperationException();
            }

            _state.Game.ProcessCardRevealed(revealData.Tag);
            ActionData data = _state.Game.GetActionData();
            bool includePartial = _config.ActionOptions[revealData.Tag].PartialPoints.HasValue;
            await RevealActionAsync(_state.Game.Players, _state.Game.CurrentArrangement, data, revealData.Tag,
                includePartial, _state.PlayerState.CardMessageId.Value, _playerChat, image);
            await RevealActionAsync(_state.Game.Players, _state.Game.CurrentArrangement, data, revealData.Tag,
                includePartial, _state.AdminState.CardMessageId.Value, _adminChat, image);
        }
        else
        {
            _state.Game.DrawQuestion();
            await RevealQuestionAsync(_state.Game, _state.PlayerState.CardMessageId.Value, _playerChat, image);
            await RevealQuestionAsync(_state.Game, _state.AdminState.CardMessageId.Value, _adminChat, image);
        }

        _saveManager.Save(_state);
    }

    internal async Task UnrevealCardAsync()
    {
        if (_state.AdminState?.CardMessageId is null || _state.PlayerState?.CardMessageId is null)
        {
            return;
        }

        if (_state.Game?.Players.Current is null)
        {
            await StartNewGameAsync();
            return;
        }

        if (!_state.Game.IsCurrentArrangementValid())
        {
            return;
        }

        string image = GetArrangementImage(_state.Game.CurrentArrangement);

        await UnrevealCardAsync(_state.Game.Players, _state.Game.CurrentArrangement!, _playerChat,
            _state.PlayerState.CardMessageId.Value, image);
        await UnrevealCardAsync(_state.Game.Players, _state.Game.CurrentArrangement!, _adminChat,
            _state.AdminState.CardMessageId.Value, image);

        _state.Game.ProcessCardUnrevealed();
        _saveManager.Save(_state);
    }

    internal Task DeleteCardAsync()
    {
        if (_state.Game?.Players.Current is null)
        {
            return StartNewGameAsync();
        }

        _state.Game.ProcessCardUnrevealed();
        _saveManager.Save(_state);

        return DeleteCardMessagesAsync();
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
        bool moved;
        if (data.Fully is null)
        {
            moved = _state.Game.CompleteQuestion();
        }
        else
        {
            fully = data.Fully.Value;
            moved = _state.Game.CompleteAction(fully);
        }

        await ShowCardAsCompletedAsync(_playerChat, _state.PlayerState.CardMessageId.Value, data.Template,
            data.HasPhoto, fully);
        await ShowCardAsCompletedAsync(_adminChat, _state.AdminState.CardMessageId.Value, data.Template,
            data.HasPhoto, fully);

        _state.ResetUserMessageId(_playerChat.Id);
        _state.ResetUserMessageId(_adminChat.Id);

        await DrawArrangementAsync(_state.Game);

        if (moved)
        {
            await ReportAndPinPlayersAsync(_state.Game);
        }
    }

    internal Task ShowRatesAsync() => _state.Game is null ? StartNewGameAsync() : ShowRatesAsync(_state.Game);

    internal Task TogglePlayersMessageStateAsync()
    {
        if (_state.Game is null)
        {
            return StartNewGameAsync();
        }

        _state.CurrentPlayersMessageState = GetNextPlayersMessageState(_state.Game.Players.GetActiveIds().Count());

        return ReportAndPinPlayersAsync(_state.Game);
    }

    internal async Task DrawCardAsync()
    {
        if (_state.Game is null)
        {
            await StartNewGameAsync();
            return;
        }

        if (!_state.Game.Players.IsActive(_state.Game.Players.Current))
        {
            bool moved = _state.Game.Players.MoveNext();
            if (!moved)
            {
                return;
            }
            await ReportAndPinPlayersAsync(_state.Game);
        }

        await DeleteCardMessagesAsync();

        await DrawArrangementAsync(_state.Game);
    }

    internal async Task ShowImagesAsync()
    {
        foreach (string[] paths in _config.Images
                                          .Values
                                          .Select(n => Path.Combine(_config.ImagesFolder, n))
                                          .Batch(_config.MaxImagesInAlbum))
        {
            await _core.UpdateSender.SendMediaGroupAsync(_adminChat, paths);
            await _core.UpdateSender.SendMediaGroupAsync(_playerChat, paths);
        }
    }

    private PlayersMessageState.Type GetNextPlayersMessageState(int activePlayers)
    {
        return PlayersMessageState.States[_state.CurrentPlayersMessageState].GetNext(activePlayers);
    }

    private async Task DeleteCardMessagesAsync()
    {
        if (_state.AdminState?.CardMessageId is null || _state.PlayerState?.CardMessageId is null)
        {
            return;
        }

        await _core.UpdateSender.DeleteMessageAsync(_playerChat, _state.PlayerState.CardMessageId.Value);
        await _core.UpdateSender.DeleteMessageAsync(_adminChat, _state.AdminState.CardMessageId.Value);

        _state.ResetUserMessageId(_playerChat.Id);
        _state.ResetUserMessageId(_adminChat.Id);
    }

    private string GetArrangementImage(Arrangement? arrangement = null, RevealCardData? revealData = null)
    {
        arrangement ??= _state.Game?.CurrentArrangement;
        return Path.Combine(_config.ImagesFolder,
            _config.GetArrangementImage(arrangement!.GetArrangementType(), revealData)!);
    }

    private Task RevealQuestionAsync(Game.States.Game game, int cardMessageId, Chat chat, string? image)
    {
        Turn turn = CreateQuestionTurn(game, chat.Id);
        MessageTemplateText template = turn.GetMessage();
        InlineKeyboardMarkup? keyboard = CreateQuestionKeyboard(chat.Id, true);
        if (keyboard is not null)
        {
            template.KeyboardProvider = keyboard;
        }

        if (image is null)
        {
            return template.EditMessageWithSelfAsync(_core.UpdateSender, chat, cardMessageId);
        }

        MessageTemplateImagePath templateImage = new(template, image);
        return templateImage.EditMessageMediaWithSelfAsync(_core.UpdateSender, chat, cardMessageId);
    }

    private Task RevealActionAsync(PlayersRepository players, Arrangement arrangement, ActionData data, string tag,
        bool showPartial, int cardMessageId, Chat chat, string image)
    {
        Texts texts = _textsProvider.GetTextsFor(chat.Id);
        string? descriptionEn = _state.ShouldIncludeEnFor(chat.Id) ? data.Descriprions[tag].en : null;
        Turn turn = new(texts, players.GetDisplayName, tag, data.Descriprions[tag].ru, descriptionEn, players.Current,
            arrangement);
        MessageTemplateText template = turn.GetMessage();
        template.KeyboardProvider = CreateActionKeyboard(chat.Id, showPartial);
        MessageTemplateImagePath templateImage = new(template, image);
        return templateImage.EditMessageMediaWithSelfAsync(_core.UpdateSender, chat, cardMessageId);
    }

    private Task UnrevealCardAsync(PlayersRepository players, Arrangement arrangement, Chat chat, int cardMessageId,
        string image)
    {
        Texts texts = _textsProvider.GetTextsFor(chat.Id);
        MessageTemplateText? partnersText = null;
        if (arrangement.Partners.Count > 0)
        {
            partnersText = Turn.GetPartnersPart(texts, arrangement, players.GetDisplayName);
        }
        MessageTemplateText template =
            texts.TurnFormatShort.Format(players.GetDisplayName(players.Current), partnersText);
        template.KeyboardProvider = CreateArrangementKeyboard(chat.Id, true);
        MessageTemplateImagePath templateImage = new(template, image);
        return templateImage.EditMessageMediaWithSelfAsync(_core.UpdateSender, chat, cardMessageId);
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

    private async Task StartNewGameAsync()
    {
        _state.Game = StartNewGame();

        Texts adminTexts = _textsProvider.GetTextsFor(_adminChat.Id);
        await adminTexts.NewGameStart.SendAsync(_core.UpdateSender, _adminChat);
        string payload = string.Format(_config.DeepLinkFormat, _core.SelfUsername, _state.Game.Guid);
        byte[] bytes = GetQr(payload);
        using (MemoryStream stream = new(bytes))
        {
            InputFileStream file = new(stream);
            MessageTemplateImageInputFile templateImage = new(adminTexts.JoinGameQrCaption, file)
            {
                ShowCaptionAboveMedia = true
            };
            await templateImage.SendAsync(_core.UpdateSender, _adminChat);
        }

        _state.PlayersMessageId = null;
        _state.CurrentPlayersMessageState = PlayersMessageState.Type.NewRearrangement;

        await ReportAndPinPlayersAsync(_state.Game);
    }

    private static byte[] GetQr(string payload)
    {
        using (QRCodeGenerator qrGenerator = new())
        {
            using (QRCodeData data = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q))
            {
                using (PngByteQRCode qr = new(data))
                {
                    return qr.GetGraphic(20);
                }
            }
        }
    }

    private async Task UpdateDecksAsync()
    {
        Texts texts = _textsProvider.GetTextsFor(_adminChat.Id);

        string firstTag = _config.ActionOptions.MinBy(p => p.Value.Points).Key;
        string secondTag = _config.ActionOptions.MaxBy(p => p.Value.Points).Key;
        _decksEquipment.Clear();
        await using (await StatusMessage.CreateAsync(_core.UpdateSender, _adminChat, texts.ReadingDecks,
                         texts.StatusMessageStartFormat, texts.StatusMessageEndFormat, GetDecksLoadStatus))
        {
            List<ActionData> actionsList = await _actionsSheet.LoadAsync<ActionData>(_config.ActionsRange);

            foreach (ActionData data in actionsList)
            {
                data.ArrangementType = new ArrangementType(data.Partners, data.CompatablePartners);

                data.Descriprions[firstTag] = (data.Description1, data.DescriptionEn1);
                data.Descriprions[secondTag] = (data.Description2, data.DescriptionEn2);

                if (data.Equipment is not null && (data.Equipment.Length > 0))
                {
                    foreach (string item in data.Equipment.Split(texts.EquipmentSeparatorSheet))
                    {
                        _decksEquipment.Add(item);
                    }
                }
            }

            Dictionary<ushort, ActionData> actions = GetIndexDictionary(actionsList);

            List<QuestionData> questionsList = await _questionsSheet.LoadAsync<QuestionData>(_config.QuestionsRange);
            Dictionary<ushort, QuestionData> questions = GetIndexDictionary(questionsList);

            _state.Core.SheetInfo = new SheetInfo(actions, questions);
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
        MessageTemplateText? equipmentPart = null;
        if (_decksEquipment.Count > 0)
        {
            string equipment =
                TextHelper.FormatAndJoin(_decksEquipment, texts.EquipmentFormat, texts.EquipmentSeparatorMessage);
            equipmentPart = texts.EquipmentPrefixFormat.Format(equipment);
        }
        return texts.StatusMessageEndSuccessFormat.Format(equipmentPart);
    }

    private Game.States.Game StartNewGame(List<AddOrUpdatePlayerData>? updates = null)
    {
        if (_state.Core.SheetInfo is null)
        {
            throw new ArgumentNullException(nameof(_state.Core.SheetInfo));
        }
        Deck<ActionData> actionDeck = new(_state.Core.SheetInfo.Actions);
        Deck<QuestionData> questionDeck = new(_state.Core.SheetInfo.Questions);

        Texts texts = _textsProvider.GetDefaultTexts();
        PlayersRepository repository = new(texts.PlayerFillNamePrefix);
        GameStatsStateCore gameStatsStateCore = new(_state.Core.ActionOptions, _state.Core.QuestionPoints, repository);
        GameStats gameStats = new(gameStatsStateCore);

        if (updates is not null && updates.Any())
        {
            gameStats.UpdateList(updates, texts.UpdatePlayerSeparator);
        }

        GroupCompatibility compatibility = new();
        DistributedMatchmaker matchmaker = new(repository, gameStats, compatibility);
        return new Game.States.Game(actionDeck, questionDeck, _config.ActionsTitle, _config.QuestionsTitle, repository,
            gameStats, matchmaker);
    }

    private async Task DrawArrangementAsync(Game.States.Game game)
    {
        game.DrawAction();
        if (game.CurrentArrangement is null)
        {
            await DrawAndSendQuestionAsync(game);
            return;
        }

        string image = GetArrangementImage(game.CurrentArrangement);
        await ShowArrangementAsync(game.Players, game.CurrentArrangement, _adminChat, image);
        await ShowArrangementAsync(game.Players, game.CurrentArrangement, _playerChat, image);

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
        InlineKeyboardMarkup? keyboard = CreateQuestionKeyboard(chat.Id, false);
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
        QuestionData data = game.GetQuestionData();
        List<string> playerIds = new() { game.Players.Current };
        if (game.CurrentArrangement is not null)
        {
            playerIds.AddRange(game.CurrentArrangement.Partners);
        }

        string? descriptionEn = _state.ShouldIncludeEnFor(userId) ? data.DescriptionEn : null;
        return
            new Turn(texts, game.Players.GetDisplayName, texts.QuestionsTag, data.Description, descriptionEn, playerIds);
    }

    private async Task ShowArrangementAsync(PlayersRepository players, Arrangement arrangement, Chat chat,
        string image)
    {
        Texts texts = _textsProvider.GetTextsFor(chat.Id);
        MessageTemplateText? partnersText = null;
        if (arrangement.Partners.Count > 0)
        {
            partnersText = Turn.GetPartnersPart(texts, arrangement, players.GetDisplayName);
        }
        MessageTemplate messageTemplate =
            texts.TurnFormatShort.Format(players.GetDisplayName(players.Current), partnersText);
        messageTemplate = new MessageTemplateImagePath(messageTemplate, image)
        {
            KeyboardProvider = CreateArrangementKeyboard(chat.Id, true)
        };
        Message message = await messageTemplate.SendAsync(_core.UpdateSender, chat);
        _state.SetUserMessageId(chat.Id, message.MessageId);

        _saveManager.Save(_state);
    }

    private Task ShowCardAsCompletedAsync(Chat chat, int messageId, MessageTemplateText original, bool hasPhoto,
        bool fully)
    {
        Texts texts = _textsProvider.GetTextsFor(chat.Id);

        string completedPart = fully ? texts.Completed : texts.ActionCompletedPartially;
        MessageTemplateText template = texts.CompletedCardFormat.Format(original, completedPart);

        return hasPhoto
            ? template.EditMessageCaptionWithSelfAsync(_core.UpdateSender, chat, false, messageId)
            : template.EditMessageWithSelfAsync(_core.UpdateSender, chat, messageId);
    }

    private Task ShowRatesAsync(Game.States.Game game)
    {
        Texts texts = _textsProvider.GetTextsFor(_adminChat.Id);
        Dictionary<string, uint> ratios = new();
        foreach (string player in game.Players.AllIds)
        {
            uint? rate = game.Stats.GetRatio(player);
            if (rate is not null)
            {
                ratios[player] = rate.Value;
            }
        }

        if (ratios.Count == 0)
        {
            return texts.NoRates.SendAsync(_core.UpdateSender, _adminChat);
        }

        uint bestRate = ratios.Values.Max();

        List<MessageTemplateText> lines = new();

        // ReSharper disable once LoopCanBePartlyConvertedToQuery
        foreach (string player in ratios.Keys.OrderByDescending(p => ratios[p]))
        {
            string name = game.Players.GetDisplayName(player, false);
            uint points = game.Stats.GetPoints(player);
            uint propositions = game.Stats.GetPropositions(player);
            uint rate = ratios[player];
            uint turns = game.Stats.GetTurns(player);

            bool enoughTurns = game.Stats.MinRound is null || (turns >= game.Stats.MinRound);
            MessageTemplateText format = enoughTurns ? texts.RateFormat : texts.RateFormatHidden;
            MessageTemplateText line = format.Format(name, points, propositions, rate, turns);
            if (rate == bestRate)
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

        List<MessageTemplateText> playerLines = new();
        List<(string Id, bool Active, byte Number)> players = new();
        foreach ((string id, bool active) in game.Players.GetAllIdsWithStatus())
        {
            if (!active)
            {
                if (_state.CurrentPlayersMessageState is not PlayersMessageState.Type.NewRearrangement
                    and not PlayersMessageState.Type.Rearrangement
                    and not PlayersMessageState.Type.Activity)
                {
                    continue;
                }
            }

            MessageTemplateText format = active ? texts.PlayerFormatActive : texts.PlayerFormatInactive;
            if (id == game.Players.Current)
            {
                MessageTemplateText currentFormat = new(texts.CurrentPlayerFormat);
                format = currentFormat.Format(format);
            }

            byte number = (byte) (playerLines.Count + 1);
            players.Add((id, active, number));
            MessageTemplateText line = format.Format(number, id);
            playerLines.Add(line);
        }

        MessageTemplateText messageText;
        if (playerLines.Any())
        {
            MessageTemplateText allLines = MessageTemplateText.JoinTexts(playerLines);

            messageText = texts.PlayersFormat.Format(allLines);

            messageText.KeyboardProvider = CreatePlayersKeyboard(texts, game.Players.Current,
                game.Players.GetActiveIds().ToList(), players);
        }
        else
        {
            messageText = texts.NoPlayersYet;
        }


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

    private InlineKeyboardMarkup CreateArrangementKeyboard(long userId, bool showActions)
    {
        Texts texts = _textsProvider.GetTextsFor(userId);

        List<List<InlineKeyboardButton>> keyboard = new();

        if (userId == _adminChat.Id)
        {
             keyboard.Add(CreateOneButtonRow<DeleteCard>(texts.Delete));
        }

        keyboard.Add(CreateOneButtonRow<RevealCard>(texts.QuestionsTag));

        if (showActions)
        {
            keyboard.AddRange(_state.Core
                                    .ActionOptions
                                    .OrderBy(o => o.Value.Points)
                                    .Select(o => CreateOneButtonRow<RevealCard>(o.Key, o.Key)));
        }

        return new InlineKeyboardMarkup(keyboard);
    }

    private InlineKeyboardMarkup CreateActionKeyboard(long userId, bool showPartial)
    {
        if (userId != _adminChat.Id)
        {
            return InlineKeyboardMarkup.Empty();
        }

        Texts texts = _textsProvider.GetTextsFor(userId);
        List<InlineKeyboardButton> adminRaw = new()
        {
            CreateButton<UnrevealCard>(texts.Unreveal),
            CreateButton<DeleteCard>(texts.Delete)
        };
        List<InlineKeyboardButton> question = CreateOneButtonRow<RevealCard>(texts.QuestionsTag);

        List<InlineKeyboardButton> partial = CreateActionButtonRow(false, userId);
        List<InlineKeyboardButton> full = CreateActionButtonRow(true, userId);

        List<List<InlineKeyboardButton>> keyboard = new()
        {
            adminRaw,
            question
        };
        if (showPartial)
        {
            keyboard.Add(partial);
        }
        keyboard.Add(full);
        return new InlineKeyboardMarkup(keyboard);
    }

    private InlineKeyboardMarkup? CreateQuestionKeyboard(long userId, bool showUnreveal)
    {
        if (userId != _adminChat.Id)
        {
            return null;
        }

        List<List<InlineKeyboardButton>> keyboard = new();

        Texts texts = _textsProvider.GetTextsFor(userId);

        List<InlineKeyboardButton> adminRaw = new();

        if (showUnreveal)
        {
            adminRaw.Add(CreateButton<UnrevealCard>(texts.Unreveal));
        }
        adminRaw.Add(CreateButton<DeleteCard>(texts.Delete));

        keyboard.Add(adminRaw);

        keyboard.Add(CreateOneButtonRow<CompleteCard>(texts.Completed));

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

    private InlineKeyboardMarkup CreatePlayersKeyboard(Texts texts, string currentPlayer,
        IReadOnlyCollection<string> activePlayers, List<(string Id, bool Active, byte Number)> players)
    {
        List<List<InlineKeyboardButton>> keyboard = new();

        if (activePlayers.Count > 0)
        {
            keyboard.Add(CreateOneButtonRow<DrawCard>(texts.DrawCard));
        }

        List<InlineKeyboardButton>? modeToggle = CreateTogglePlayersMessageStateRow(activePlayers.Count, texts);
        if (modeToggle is not null)
        {
            keyboard.Add(modeToggle);
        }

        List<InlineKeyboardButton> playerButtons = new();
        foreach ((string? id, bool active, byte number) in players)
        {
            InlineKeyboardButton button;
            switch (_state.CurrentPlayersMessageState)
            {
                case PlayersMessageState.Type.NewRearrangement:
                    button = CreateButton<RearrangePlayer>(number.ToString(), id);
                    break;

                case PlayersMessageState.Type.Rearrangement:
                    string format = active ? texts.ActivePlayerFormat : texts.InactivePlayerFormat;
                    button = CreateButton<RearrangePlayer>(string.Format(format, number), id);
                    break;

                case PlayersMessageState.Type.Activity:
                    format = active ? texts.ActivePlayerFormat : texts.InactivePlayerFormat;
                    button = CreateButton<TogglePlayer>(string.Format(format, number), id);
                    break;

                case PlayersMessageState.Type.Selection:
                    if (!active || (id == currentPlayer))
                    {
                        continue;
                    }
                    button = CreateButton<SelectPlayer>(number.ToString(), id);
                    break;

                case PlayersMessageState.Type.FastMovement:
                    if (!active || (id == activePlayers.LastOrDefault()))
                    {
                        continue;
                    }
                    button = CreateButton<MovePlayerToBottom>(number.ToString(), id);
                    break;

                case PlayersMessageState.Type.Movement:
                    if (!active)
                    {
                        continue;
                    }
                    button = CreateButton<MovePlayerDown>(number.ToString(), id);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
            playerButtons.Add(button);
        }
        keyboard.AddRange(playerButtons.Batch(_config.ButtonsPerRow)
                                       .Select(b => b.ToList()));

        return keyboard.Count == 0 ? InlineKeyboardMarkup.Empty() : new InlineKeyboardMarkup(keyboard);
    }

    private List<InlineKeyboardButton>? CreateTogglePlayersMessageStateRow(int activePlayers, Texts texts)
    {
        string current = PlayersMessageState.GetLabel(_state.CurrentPlayersMessageState, texts);
        string next = PlayersMessageState.GetLabel(GetNextPlayersMessageState(activePlayers), texts);

        if (current == next)
        {
            return null;
        }

        string caption = string.Format(texts.PlayersMessageStatesFormat, current, next);
        return CreateOneButtonRow<TogglePlayersMessageState>(caption);
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
    private readonly Chat _adminChat;
    private readonly Chat _playerChat;
    private readonly Manager _sheetsManager;
    private readonly SaveManager<BotState, BotData> _saveManager;
    private readonly ITextsProvider<Texts> _textsProvider;

    private readonly BotCore _core;
    private readonly Config _config;
}