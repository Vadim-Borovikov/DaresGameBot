using System;
using System.Collections.Generic;
using System.Linq;
using AbstractBot;
using DaresGameBot.Configs;
using DaresGameBot.Game.Matchmaking;
using DaresGameBot.Helpers;

namespace DaresGameBot.Game.Data;

internal sealed class Game : Context
{
    public Matchmaker Matchmaker;
    public IEnumerable<string> PlayerNames => _players.Select(p => p.Name);
    public bool Fresh;

    public bool IsActive => _nextActionTurn is not null;

    public Game(Config config, List<Player> players, Matchmaker matchmaker, IList<Deck<CardAction>> actionDecks,
        Deck<Card> questionsDeck)
    {
        Fresh = true;
        _config = config;
        _actionDecks = actionDecks;
        _questionsDeck = questionsDeck;

        UpdatePlayers(players);
        Matchmaker = matchmaker;

        TryPrepareNextActionTurn();
    }

    public Turn Draw(Func<Deck<Card>> questionsDeckCreator, bool action = true)
    {
        if (action)
        {
            if (!Fresh)
            {
                _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
            }

            Fresh = false;
            Turn? result = _nextActionTurn;
            TryPrepareNextActionTurn();
            return result!;
        }

        Turn? turn = DrawQuestion();
        if (turn is null)
        {
            _questionsDeck = questionsDeckCreator();
            turn = DrawQuestion()!;
        }

        Fresh = false;
        return turn;
    }

    public void UpdatePlayers(List<Player> players)
    {
        _players = players;
        _currentPlayerIndex = 0;
    }

    private Turn? DrawQuestion() => _questionsDeck.TryGetTurn(TryCreateQuestionTurn);

    private Turn TryCreateQuestionTurn(Card card)
    {
        return new Turn(_config.Texts, _config.ImagesFolder, _questionsDeck.Tag, card.Description);
    }

    private Turn? TryCreateActionTurn(Player player, CardAction card)
    {
        List<Player>? partners = null;
        if (card.Partners > 0)
        {
            partners = Matchmaker.EnumerateMatches(player, _players, card.Partners, card.CompatablePartners)?.ToList();
            if (partners is null)
            {
                return null;
            }
        }

        List<Player>? helpers = null;
        if (card.Helpers > 0)
        {
            List<Player> choices =
                _players.Where(p => (p != player) && (partners is null || !partners.Contains(p))).ToList();
            helpers = RandomHelper.EnumerateUniqueItems(Random.Shared, choices, card.Helpers)?.ToList();
            if (helpers is null)
            {
                return null;
            }
        }

        if (!card.AssignPartners)
        {
            partners = null;
        }

        return new Turn(_config.Texts, _config.ImagesFolder, card.Tag, card.Description, player, card.ImagePath,
            partners, helpers);
    }

    private void TryPrepareNextActionTurn()
    {
        int nextPlayerIndex = Fresh ? 0 : (_currentPlayerIndex + 1) % _players.Count;
        Player nextPlayer = _players[nextPlayerIndex];
        while (_actionDecks.Any())
        {
            Deck<CardAction> deck = _actionDecks.First();
            Turn? turn = deck.TryGetTurn(c => TryCreateActionTurn(nextPlayer, c));
            if (turn is not null)
            {
                _nextActionTurn = turn;
                return;
            }
            _actionDecks.RemoveAt(0);
        }

        _nextActionTurn = null;
    }

    private readonly Config _config;
    private readonly IList<Deck<CardAction>> _actionDecks;
    private Deck<Card> _questionsDeck;
    private List<Player> _players = new();
    private int _currentPlayerIndex;
    private Turn? _nextActionTurn;
}
