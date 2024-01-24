﻿using System;
using System.Collections.Generic;
using System.Linq;
using AbstractBot;

namespace DaresGameBot.Game.Data;

internal sealed class Game : Context
{
    public IEnumerable<string> PlayerNames => _players.Select(p => p.Name);
    public bool Fresh;

    public bool IsActive => _nextActionTurn is not null;

    public Game(List<Player> players, IList<Deck<CardAction>> actionDecks, Deck<Card> questionsDeck, Random random)
    {
        Fresh = true;
        _actionDecks = actionDecks;
        _questionsDeck = questionsDeck;
        _random = random;

        UpdatePlayers(players);

        TryPrepareNextActionTurn();
    }

    public Turn? Draw(Func<Deck<Card>> questionsDeckCreator, bool action = true)
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
            return result;
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

    private Turn? DrawQuestion()
    {
        Player player = _players[_currentPlayerIndex];
        return _questionsDeck.TryGetTurn(player, TryCreateQuestionTurn);
    }

    private Turn TryCreateQuestionTurn(Player player, Card card)
    {
        return new Turn(_questionsDeck.Tag, card.Description, player);
    }

    private Turn? TryCreateActionTurn(Player player, CardAction card)
    {
        List<Player>? partners = null;
        if (card.Partners > 0)
        {
            Player[] choices = _players.Where(p => p.IsCompatableWith(player)).ToArray();
            if (choices.Length < card.Partners)
            {
                return null;
            }

            _random.Shuffle(choices);
            if (card.CompatablePartners)
            {
                partners =
                    EnumerateSubgroups(choices.ToList(), card.Partners).FirstOrDefault(Player.AreCompatable);
                if (partners is null)
                {
                    return null;
                }
            }
            else
            {
                partners = new List<Player>(choices.Take(card.Partners));
            }
        }

        List<Player>? helpers = null;
        if (card.Helpers > 0)
        {
            Player[] choices =
                _players.Where(p => (p != player) && (partners is null || !partners.Contains(p))).ToArray();
            if (choices.Length < card.Helpers)
            {
                return null;
            }

            _random.Shuffle(choices);
            helpers = new List<Player>(choices.Take(card.Helpers));
        }

        if (!card.AssignPartners)
        {
            partners = null;
        }

        return new Turn(card.Tag, card.Description, player, partners, helpers);
    }

    private void TryPrepareNextActionTurn()
    {
        int nextPlayerIndex = Fresh ? 0 : (_currentPlayerIndex + 1) % _players.Count;
        Player nextPlayer = _players[nextPlayerIndex];
        while (_actionDecks.Any())
        {
            Deck<CardAction> deck = _actionDecks.First();
            Turn? turn = deck.TryGetTurn(nextPlayer, TryCreateActionTurn);
            if (turn is not null)
            {
                _nextActionTurn = turn;
                return;
            }
            _actionDecks.RemoveAt(0);
        }

        _nextActionTurn = null;
    }

    private static IEnumerable<List<Player>> EnumerateSubgroups(List<Player> choices, int size)
    {
        if (choices.Count == size)
        {
            yield return choices;
        }
        else if (choices.Count > size)
        {
            for (int i = 0; i < choices.Count; i++)
            {
                List<Player> subset = new(choices);
                subset.RemoveAt(i);
                foreach (List<Player> subsetOfSubset in EnumerateSubgroups(subset, size))
                {
                    yield return subsetOfSubset;
                }
            }
        }
    }

    private readonly Random _random;
    private readonly IList<Deck<CardAction>> _actionDecks;
    private Deck<Card> _questionsDeck;
    private List<Player> _players = new();
    private int _currentPlayerIndex;
    private Turn? _nextActionTurn;
}
