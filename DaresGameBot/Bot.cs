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

namespace DaresGameBot;

public sealed class Bot : BotWithSheets<Config, Texts, object, CommandDataSimple>
{
    public Bot(Config config) : base(config)
    {
        Operations.Add(new UpdateCommand(this));
        Operations.Add(new NewCommand(this));
        Operations.Add(new DrawActionCommand(this));
        Operations.Add(new DrawQuestionCommand(this));
        Operations.Add(new LangCommand(this));
        Operations.Add(new UpdatePlayers(this));

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
            List<CardAction> actions = await _actionsSheet.LoadAsync<CardAction>(Config.ActionsRange);
            List<Card> questions = await _questionsSheet.LoadAsync<Card>(Config.QuestionsRange);

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

        game.UpdatePlayers(updates);

        MessageTemplateText playersText =
            Config.Texts.PlayersFormat.Format(string.Join(PlayerSeparator, game.Players));
        MessageTemplateText messageText = Config.Texts.AcceptedFormat.Format(playersText);
        return messageText.SendAsync(this, chat);
    }

    internal Task OnNewGameAsync(Chat chat, User sender)
    {
        Contexts.Remove(sender.Id);
        MessageTemplateText message = Config.Texts.NewGame;
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

    private Game.Data.Game StartNewGame(List<PlayerListUpdate> updates)
    {
        if (_decksProvider is null)
        {
            throw new ArgumentNullException(nameof(_decksProvider));
        }

        PlayerRepository repository = new(updates, Config.Points);
        DistributedMatchmaker matchmaker = new(repository);

        List<IInteractionSubscriber> subscribers = new()
        {
            repository,
            matchmaker.InteractionRepository
        };

        return new Game.Data.Game(Config, _decksProvider, repository, matchmaker, subscribers);
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

    private static ReplyKeyboardMarkup GetKeyboard(params string[] buttonCaptions)
    {
        return new ReplyKeyboardMarkup(buttonCaptions.Select(c => new KeyboardButton(c)));
    }

    private Game.DecksProvider? _decksProvider;
    private readonly Sheet _actionsSheet;
    private readonly Sheet _questionsSheet;

    private const string PlayerSeparator = ", ";
}