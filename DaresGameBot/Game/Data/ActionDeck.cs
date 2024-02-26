using System;
using System.Collections.Generic;

namespace DaresGameBot.Game.Data;

internal sealed class ActionDeck
{
    public ActionDeck(IReadOnlyList<CardAction> source, List<int> indices)
    {
        _source = source;
        _indices = indices;
    }

    public Turn? TryGetTurn(Func<CardAction, Turn?> creator)
    {
        foreach (int i in _indices)
        {
            CardAction card = _source[i];
            Turn? turn = creator(card);
            if (turn is not null)
            {
                _indices.Remove(i);
                return turn;
            }
        }

        return null;
    }

    private readonly IReadOnlyList<CardAction> _source;
    private readonly List<int> _indices;
}