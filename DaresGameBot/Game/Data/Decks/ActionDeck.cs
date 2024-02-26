using System;
using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Game.Data.Cards;
using DaresGameBot.Helpers;

namespace DaresGameBot.Game.Data.Decks;

internal sealed class ActionDeck
{
    public ActionDeck(IEnumerable<CardAction> source)
    {
        CardAction[] cards = RandomHelper.Shuffle(source);
        _cards = cards.ToList();
    }

    public Turn? TryGetTurn(Func<CardAction, Turn?> creator)
    {
        for (int i = 0; i < _cards.Count; ++i)
        {
            CardAction card = _cards[i];
            Turn? turn = creator(card);
            if (turn is not null)
            {
                _cards.RemoveAt(i);
                return turn;
            }
        }

        return null;
    }

    private readonly List<CardAction> _cards;
}