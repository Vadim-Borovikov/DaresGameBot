using System;
using System.Collections.Generic;

namespace DaresGameBot.Game.Data;

internal sealed class Deck<T> where T : Card
{
    public Deck(IReadOnlyList<T> allCards, List<int> indices)
    {
        _allCards = allCards;
        _indices = indices;
    }

    public Turn? TryGetTurn(Func<T, Turn?> creator)
    {
        foreach (int i in _indices)
        {
            T card = _allCards[i];
            Turn? turn = creator(card);
            if (turn is not null)
            {
                _indices.Remove(i);
                return turn;
            }
        }

        return null;
    }

    private readonly List<int> _indices;
    private readonly IReadOnlyList<T> _allCards;
}