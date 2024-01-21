using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AbstractBot.Configs.MessageTemplates;
using DaresGameBot.Game.Data;
using Telegram.Bot.Types;

namespace DaresGameBot.Game;

internal sealed class Manager
{
    public Manager(Bot bot, List<CardAction> actions, List<Card> questions)
    {
        _bot = bot;
        _actions = actions;
        _questions = questions;
    }

    public Data.Game StartNewGame(List<Player> players)
    {
        IList<Deck<CardAction>> actionDecks = GetActionDecks();
        Deck<Card> questionsDeck = CreateQuestionsDeck();
        return new Data.Game(players, actionDecks, questionsDeck, _random);
    }

    public Task RepotNewGameAsync(Chat chat, Data.Game game)
    {
        MessageTemplateText playersText =
            _bot.Config.Texts.PlayersFormat.Format(string.Join(PlayerSeparator, game.PlayerNames));
        MessageTemplateText startText = _bot.Config.Texts.NewGameFormat.Format(playersText);
        return startText.SendAsync(_bot, chat);
    }

    private IList<Deck<CardAction>> GetActionDecks()
    {
        ReadOnlyCollection<CardAction> cards = _actions.AsReadOnly();
        return _actions.GroupBy(c => c.Tag).Select(g => CreateActionDeck(cards, g.Key)).ToList();
    }

    private Deck<CardAction> CreateActionDeck(IReadOnlyList<CardAction> cards, string tag)
    {
        List<int> indices = new();
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i].Tag.Equals(tag, StringComparison.OrdinalIgnoreCase))
            {
                indices.Add(i);
            }
        }

        int[] indexArray = indices.ToArray();
        _random.Shuffle(indexArray);
        return new Deck<CardAction>(tag, cards, indexArray.ToList());
    }

    private Deck<Card> CreateQuestionsDeck()
    {
        ReadOnlyCollection<Card> cards = _questions.AsReadOnly();
        List<int> indices = Enumerable.Range(0, cards.Count).ToList();
        return new Deck<Card>(_bot.Config.Texts.QuestionsTag, cards, indices);
    }

    public Task UpdatePlayersAsync(Chat chat, Data.Game game, List<Player> players)
    {
        game.UpdatePlayers(players);

        MessageTemplateText playersText =
            _bot.Config.Texts.PlayersFormat.Format(string.Join(PlayerSeparator, game.PlayerNames));
        MessageTemplateText messageText = _bot.Config.Texts.AcceptedFormat.Format(playersText);
        return messageText.SendAsync(_bot, chat);
    }

    public Turn? Draw(Data.Game game, bool action = true)
    {
        if (action)
        {
            if (!game.Fresh)
            {
                game.SwitchPlayer();
            }

            game.Fresh = false;
            return game.DrawAction();
        }

        Turn? turn = game.DrawQuestion();
        if (turn is null)
        {
            game.SetQuestions(CreateQuestionsDeck());
            turn = game.DrawQuestion()!;
        }

        game.Fresh = false;
        return turn;
    }

    public async Task RepotTurnAsync(Chat chat, Data.Game game, Turn turn, int replyToMessageId)
    {
        MessageTemplateText message = turn.GetMessage(game.PlayerNames.Count());
        message.ReplyToMessageId = replyToMessageId;
        await message.SendAsync(_bot, chat);
    }

    private readonly Bot _bot;
    private readonly List<CardAction> _actions;
    private readonly List<Card> _questions;
    private readonly Random _random = new();

    private const string PlayerSeparator = ", ";
}