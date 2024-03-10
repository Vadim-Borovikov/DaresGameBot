using DaresGameBot.Operations;
using GoogleSheetsManager.Documents;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AbstractBot;
using AbstractBot.Bots;
using AbstractBot.Operations.Data;
using DaresGameBot.Operations.Commands;
using DaresGameBot.Configs;
using DaresGameBot.Game.Data;
using System.Collections.Generic;
using Telegram.Bot.Types.Enums;
using AbstractBot.Configs.MessageTemplates;
using DaresGameBot.Game.Matchmaking;
using DaresGameBot.Game.Data.Cards;
using DaresGameBot.Game.Data.Players;
using System;

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
        Operations.Add(new UpdatePlayersOperation(this));

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

            _manager = new Game.Manager(this, actions, questions);
        }

        Contexts.Remove(chat.Id);
    }

    internal async Task UpdatePlayersAsync(Chat chat, List<Player> players,
        Dictionary<string, IInteractabilityProvider> infos)
    {
        Game.Data.Game? game = TryGetContext<Game.Data.Game>(chat.Id);
        if (game is null)
        {
            Contexts[chat.Id] = await StartNewGameAsync(chat, players, infos);
            return;
        }

        if (_manager is null)
        {
            throw new ArgumentNullException(nameof(_manager));
        }

        await _manager.UpdatePlayersAsync(chat, game, players, infos);
    }

    internal Task OnNewGameAsync(Chat chat)
    {
        Contexts.Remove(chat.Id);
        MessageTemplateText message = Config.Texts.NewGame;
        message.KeyboardProvider = KeyboardProvider.Remove;
        return message.SendAsync(this, chat);
    }

    internal Task DrawAsync(Chat chat, int replyToMessageId, bool action = true)
    {
        Game.Data.Game? game = TryGetContext<Game.Data.Game>(chat.Id);
        if (game is null)
        {
            return OnNewGameAsync(chat);
        }

        return
            action ? DrawActionAsync(game, chat, replyToMessageId) : DrawQuestionAsync(game, chat, replyToMessageId);
    }

    internal Task OnToggleLanguagesAsync(Chat chat)
    {
        Game.Data.Game? game = TryGetContext<Game.Data.Game>(chat.Id);
        if (game is null)
        {
            return OnNewGameAsync(chat);
        }

        game.ToggleLanguages();

        MessageTemplateText message = game.IncludeEn ? Config.Texts.LangToggledToRuEn : Config.Texts.LangToggledToRu;
        return message.SendAsync(this, chat);
    }

    private Task DrawActionAsync(Game.Data.Game game, Chat chat, int replyToMessageId)
    {
        Turn? turn = game.TryDrawAction();
        if (turn is not null)
        {
            if (_manager is null)
            {
                throw new ArgumentNullException(nameof(_manager));
            }

            return _manager.RepotTurnAsync(chat, game, turn, replyToMessageId);
        }

        return game.Status switch
        {
            Game.Data.Game.ActionDecksStatus.InDeck        => DrawQuestionAsync(game, chat, replyToMessageId, true),
            Game.Data.Game.ActionDecksStatus.BeforeDeck    => Config.Texts.DeckEnded.SendAsync(this, chat),
            Game.Data.Game.ActionDecksStatus.AfterAllDecks => Config.Texts.GameOver.SendAsync(this, chat),
            _                                              => throw new ArgumentOutOfRangeException()
        };
    }

    private Task DrawQuestionAsync(Game.Data.Game game, Chat chat, int replyToMessageId,
        bool forPlayerWithNoMatches = false)
    {
        Turn turn = game.DrawQuestion(forPlayerWithNoMatches);

        if (_manager is null)
        {
            throw new ArgumentNullException(nameof(_manager));
        }

        return _manager.RepotTurnAsync(chat, game, turn, replyToMessageId);
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

    private async Task<Game.Data.Game> StartNewGameAsync(Chat chat, List<Player> players,
        Dictionary<string, IInteractabilityProvider> infos)
    {
        Compatibility compatibility = new(infos);

        if (_manager is null)
        {
            throw new ArgumentNullException(nameof(_manager));
        }

        Game.Data.Game game = _manager.StartNewGame(players, compatibility);
        Contexts[chat.Id] = game;
        await _manager.RepotNewGameAsync(chat, game);
        return game;
    }

    private static ReplyKeyboardMarkup GetKeyboard(params string[] buttonCaptions)
    {
        return new ReplyKeyboardMarkup(buttonCaptions.Select(c => new KeyboardButton(c)));
    }

    private Game.Manager? _manager;
    private readonly Sheet _actionsSheet;
    private readonly Sheet _questionsSheet;
}