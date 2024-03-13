using AbstractBot;
using AbstractBot.Bots;
using AbstractBot.Configs.MessageTemplates;
using AbstractBot.Extensions;
using DaresGameBot.Configs;
using DaresGameBot.Game.Data;
using DaresGameBot.Game.Data.Cards;
using DaresGameBot.Game.Matchmaking;
using DaresGameBot.Game.Matchmaking.PlayerCheck;
using DaresGameBot.Operations;
using DaresGameBot.Operations.Commands;
using GoogleSheetsManager.Documents;
using GryphonUtilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DaresGameBot.Operations.Info;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DaresGameBot;

public sealed class Bot : BotWithSheets<Config, Texts, object, StartData>
{
    public Bot(Config config) : base(config)
    {
        Operations.Add(new UpdateCommand(this));
        Operations.Add(new NewCommand(this));
        Operations.Add(new DrawActionCommand(this));
        Operations.Add(new DrawQuestionCommand(this));
        Operations.Add(new LangCommand(this));
        Operations.Add(new UpdatePlayers(this));

        Operations.Add(new StartGameWithPersonalPreferences(this));
        AllPreferencesCommand allPreferencesCommand = new(this);
        _allPreferencesCommandName = allPreferencesCommand.BotCommand.Command;
        Operations.Add(allPreferencesCommand);
        MyPreferencesCommand myPreferencesCommand = new(this);
        _myPreferencesCommandName = myPreferencesCommand.BotCommand.Command;
        Operations.Add(myPreferencesCommand);
        Operations.Add(new UpdateName(this));
        Operations.Add(new TogglePreference(this));

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

    protected override Task OnStartCommand(StartData info, Message message, User sender)
    {
        Game.Data.Game? game = Contexts.FilterByValueType<long, object, Game.Data.Game>()
                                       .Values
                                       .FirstOrDefault(l => l.Id == info.GameId);
        Chat chat = message.Chat;
        if (game is null)
        {
            return Config.Texts.NoGameFound.SendAsync(this, chat);
        }
        if (!game.CanBeJoined)
        {
            return Config.Texts.GameCanNotBeJoined.SendAsync(this, chat);
        }

        Contexts[sender.Id] = game;

        return AddPlayerAsync(chat, game);
    }

    internal async Task UpdateDecksAsync(Chat chat)
    {
        await using (await StatusMessage.CreateAsync(this, chat, Config.Texts.ReadingDecks))
        {
            List<CardAction> actions = await _actionsSheet.LoadAsync<CardAction>(Config.ActionsRange);
            List<Card> questions = await _questionsSheet.LoadAsync<Card>(Config.QuestionsRange);

            _decksProvider = new Game.DecksProvider(actions, questions);
        }

        Contexts.Remove(chat.Id);
    }

    internal Task UpdatePlayersAsync(Chat chat, User sender, IEnumerable<string> players,
        Dictionary<string, IPartnerChecker> infos)
    {
        Game.Data.Game? game = TryGetContext<Game.Data.Game>(sender.Id);
        if (game is null)
        {
            game = StartNewGame(players, infos);
            Contexts[sender.Id] = game;
            return ReportNewGameAsync(chat, game);
        }

        game.UpdatePlayers(players, infos);

        MessageTemplateText playersText =
            Config.Texts.PlayersFormat.Format(string.Join(PlayerSeparator, game.Players));
        MessageTemplateText messageText = Config.Texts.AcceptedFormat.Format(playersText);
        return messageText.SendAsync(this, chat);
    }

    internal async Task StartGameWithPersonalPreferences(Chat chat, User sender)
    {
        Game.Data.Game game = StartNewGame(sender.FirstName, chat);
        Contexts[sender.Id] = game;

        Config.Texts.NewGameLink.KeyboardProvider = KeyboardProvider.Remove;
        await Config.Texts.NewGameLink.SendAsync(this, chat);

        string link = string.Format(LinkFormat, User?.Username!, game.Id);
        await SendTextMessageAsync(chat, link, KeyboardProvider.Remove);

        await ShowPlayersSoFar(game, chat);
    }

    internal Task OnNewGameAsync(Chat chat, User sender)
    {
        Contexts.Remove(sender.Id);
        MessageTemplateText message = Config.Texts.NewGame;
        message.KeyboardProvider = GetNewGameKeyboard();
        return message.SendAsync(this, chat);
    }

    internal Task DrawAsync(Chat chat, User sender, int replyToMessageId, bool action = true)
    {
        Game.Data.Game? game = TryGetContext<Game.Data.Game>(sender.Id);
        if (game is null)
        {
            return OnNewGameAsync(chat, sender);
        }

        return
            action ? DrawActionAsync(chat, game, replyToMessageId) : DrawQuestionAsync(chat, game, replyToMessageId);
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

    internal async Task AddPlayerAsync(Chat chat, Game.Data.Game game, string? player = null)
    {
        player ??= chat.FirstName!;
        if (game.Players.Contains(player))
        {
            MessageTemplateText messageText = Config.Texts.PlayerDeclinedNameFormat.Format(player);
            messageText.KeyboardProvider = KeyboardProvider.Remove;
            await messageText.SendAsync(this, chat);
            return;
        }

        PersonalChecker checker = new(chat);
        game.AddPlayer(player, checker);
        await ShowPlayersSoFar(game);

        Config.Texts.PlayerAccepted.KeyboardProvider = KeyboardProvider.Remove;
        await Config.Texts.PlayerAccepted.SendAsync(this, chat);
        if (game.Status == Game.Data.Game.ActionDecksStatus.InDeck)
        {
            await UpdateAllPreferencesAsync(game);
        }
    }

    internal Task UpdatePreferencesAsync(Chat chat)
    {
        Game.Data.Game? game = TryGetContext<Game.Data.Game>(chat.Id);
        if (game is null)
        {
            return Config.Texts.NoGameFound.SendAsync(this, chat);
        }

        Dictionary<string, PersonalChecker> infos = GetPersonalCheckers(game);
        PersonalChecker? info = infos.Values.FirstOrDefault(i => i.Id == chat.Id);
        if (info is null)
        {
            return Config.Texts.NoGameFound.SendAsync(this, chat);
        }

        info.PreferencesMessage = null;
        return UpdatePreferencesAsync(info, infos);
    }

    internal Task UpdateAllPreferencesAsync(Chat chat)
    {
        Game.Data.Game? game = TryGetContext<Game.Data.Game>(chat.Id);
        return game is null ? Config.Texts.NoGameFound.SendAsync(this, chat) : UpdateAllPreferencesAsync(game);
    }

    internal Task TogglePreferenceAsync(Chat chat, long partnerId)
    {
        Game.Data.Game? game = TryGetContext<Game.Data.Game>(chat.Id);
        if (game is null)
        {
            return Config.Texts.NoGameFound.SendAsync(this, chat);
        }

        Dictionary<string, PersonalChecker> infos = GetPersonalCheckers(game);
        PersonalChecker? info = infos.Values.FirstOrDefault(i => i.Id == chat.Id);
        if (info is null)
        {
            return Config.Texts.NoGameFound.SendAsync(this, chat);
        }

        info.Toggle(partnerId);
        game.OnPlayersChanged();

        return UpdatePreferencesAsync(info, infos);
    }

    private static Dictionary<string, PersonalChecker> GetPersonalCheckers(Game.Data.Game game)
    {
        return game.CompanionsSelector
                   .Matchmaker
                   .Compatibility
                   .PlayerInfos
                   .FilterByValueType<string, IPartnerChecker, PersonalChecker>();
    }

    private async Task UpdateAllPreferencesAsync(Game.Data.Game game)
    {
        Dictionary<string, PersonalChecker> infos = GetPersonalCheckers(game);
        foreach (PersonalChecker info in infos.Values)
        {
            info.PreferencesMessage = null;
            await UpdatePreferencesAsync(info, infos);
        }
    }

    protected override KeyboardProvider GetDefaultKeyboardProvider(Chat chat)
    {
        Game.Data.Game? game = TryGetContext<Game.Data.Game>(chat.Id);

        if (game is null || (game.Status == Game.Data.Game.ActionDecksStatus.AfterAllDecks))
        {
            return GetKeyboard(Config.Texts.NewGameCaption);
        }

        return GetKeyboard(Config.Texts.DrawActionCaption, Config.Texts.DrawQuestionCaption);
    }

    private Task ReportNewGameAsync(Chat chat, Game.Data.Game game)
    {
        MessageTemplateText playersText =
            Config.Texts.PlayersFormat.Format(string.Join(PlayerSeparator, game.Players));
        MessageTemplateText startText = Config.Texts.NewGameFormat.Format(playersText);
        return startText.SendAsync(this, chat);
    }

    private Game.Data.Game StartNewGame(IEnumerable<string> players, Dictionary<string, IPartnerChecker> infos)
    {
        if (_decksProvider is null)
        {
            throw new ArgumentNullException(nameof(_decksProvider));
        }

        Compatibility compatibility = new(infos);
        DistributedMatchmaker matchmaker = new(compatibility);

        return new Game.Data.Game(Config, _decksProvider, matchmaker, matchmaker.InteractionRepository, players);
    }

    private Game.Data.Game StartNewGame(string player, Chat chat)
    {
        if (_decksProvider is null)
        {
            throw new ArgumentNullException(nameof(_decksProvider));
        }

        Compatibility compatibility = new();
        DistributedMatchmaker matchmaker = new(compatibility);

        PersonalChecker checker = new(chat);

        return
            new Game.Data.Game(Config, _decksProvider, matchmaker, matchmaker.InteractionRepository, player, checker);
    }

    private Task DrawActionAsync(Chat chat, Game.Data.Game game, int replyToMessageId)
    {
        Turn? turn = game.TryDrawAction();
        if (turn is not null)
        {
            return RepotTurnAsync(chat, game, turn, replyToMessageId);
        }

        return game.Status switch
        {
            Game.Data.Game.ActionDecksStatus.InDeck        => DrawQuestionAsync(chat, game, replyToMessageId, true),
            Game.Data.Game.ActionDecksStatus.BeforeDeck    => Config.Texts.DeckEnded.SendAsync(this, chat),
            Game.Data.Game.ActionDecksStatus.AfterAllDecks => Config.Texts.GameOver.SendAsync(this, chat),
            _                                              => throw new ArgumentOutOfRangeException()
        };
    }

    private Task DrawQuestionAsync(Chat chat, Game.Data.Game game, int replyToMessageId,
        bool forPlayerWithNoMatches = false)
    {
        Turn turn = game.DrawQuestion(forPlayerWithNoMatches);
        return RepotTurnAsync(chat, game, turn, replyToMessageId);
    }

    private async Task RepotTurnAsync(Chat chat, Game.Data.Game game, Turn turn, int replyToMessageId)
    {
        MessageTemplate message = turn.GetMessage(game.Players.Count, game.IncludeEn);
        message.ReplyToMessageId = replyToMessageId;
        await message.SendAsync(this, chat);
    }

    private async Task ShowPlayersSoFar(Game.Data.Game game, Chat? chat = null)
    {
        List<MessageTemplateText> names = game.Players.Select(p => Config.Texts.PlayerFormat.Format(p)).ToList();
        MessageTemplateText players = MessageTemplateText.JoinTexts(names);
        MessageTemplateText message = Config.Texts.CurrentPlayersFormat.Format(players, _allPreferencesCommandName);
        if (chat is null)
        {
            await EditMessageTextAsync(_playersMessage!.Chat, _playersMessage.MessageId, message.EscapeIfNeeded(),
                ParseMode.MarkdownV2);
            return;
        }

        message.KeyboardProvider = KeyboardProvider.Same;
        _playersMessage = await message.SendAsync(this, chat);
        await UpdatePinAsync(chat, _playersMessage.MessageId);
    }

    private async Task UpdatePinAsync(Chat chat, int messageId)
    {
        await UnpinAllChatMessagesAsync(chat);
        await PinChatMessageAsync(chat, messageId);
        await DeleteMessageAsync(chat, messageId + 1);
    }

    private async Task UpdatePreferencesAsync(PersonalChecker me, Dictionary<string, PersonalChecker> all)
    {
        MessageTemplateText messageTemplate =
            Config.Texts.SetCompatabilityFormat.Format(Config.Texts.GetPreferences(), _myPreferencesCommandName);
        InlineKeyboardMarkup keyboard = GetPreferenceKeyboard(me, all);
        if (me.PreferencesMessage is not null)
        {
            await EditMessageTextAsync(me.Chat, me.PreferencesMessage.MessageId, messageTemplate.EscapeIfNeeded(),
                ParseMode.MarkdownV2, replyMarkup: keyboard);
            return;
        }

        messageTemplate.KeyboardProvider = keyboard;
        me.PreferencesMessage = await messageTemplate.SendAsync(this, me.Chat);
    }

    private InlineKeyboardMarkup GetNewGameKeyboard()
    {
        InlineKeyboardButton button = new(Config.Texts.StartGameAndGetLink)
        {
            CallbackData = nameof(DaresGameBot.Operations.StartGameWithPersonalPreferences)
        };
        return new InlineKeyboardMarkup(button);
    }

    private InlineKeyboardMarkup GetPreferenceKeyboard(PersonalChecker me, Dictionary<string, PersonalChecker> all)
    {
        IEnumerable<IEnumerable<InlineKeyboardButton>> rows =
            all.Where(p => p.Value.Id != me.Id).Select(o => GetPreferenceButton(me, o.Key, o.Value).Yield());
        return new InlineKeyboardMarkup(rows);
    }

    private InlineKeyboardButton GetPreferenceButton(PersonalChecker me, string other, PersonalChecker otherInfo)
    {
        CompatabilityInfo info =
            me.CompatablePlayerIds.Contains(otherInfo.Id) ? Config.Texts.Compatable : Config.Texts.NotCompatable;
        string caption = string.Format(Config.Texts.CompatabilityButtonCaptionFormat, other, info.Sign);
        return new InlineKeyboardButton(caption)
        {
            CallbackData = $"{nameof(TogglePreference)}{otherInfo.Id}"
        };
    }

    private static ReplyKeyboardMarkup GetKeyboard(params string[] buttonCaptions)
    {
        return new ReplyKeyboardMarkup(buttonCaptions.Select(c => new KeyboardButton(c)));
    }

    private Game.DecksProvider? _decksProvider;
    private readonly Sheet _actionsSheet;
    private readonly Sheet _questionsSheet;
    private Message? _playersMessage;
    private readonly string _allPreferencesCommandName;
    private readonly string _myPreferencesCommandName;

    private const string LinkFormat = "https://t.me/{0}?start={1}";
    private const string PlayerSeparator = ", ";
}