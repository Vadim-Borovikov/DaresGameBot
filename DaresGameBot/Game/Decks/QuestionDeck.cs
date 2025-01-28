using System.Collections.Generic;
using DaresGameBot.Game.Data;
using DaresGameBot.Helpers;

namespace DaresGameBot.Game.Decks;

internal sealed class QuestionDeck
{
    public QuestionDeck(IReadOnlyList<CardData> cards)
    {
        _all = cards;
        _current = new Queue<CardData>();
    }

    public CardData Draw()
    {
        if (_current.Count == 0)
        {
            IEnumerable<CardData> items = RandomHelper.Shuffle(_all);
            _current = new Queue<CardData>(items);
        }

        return _current.Dequeue();
    }

    private readonly IReadOnlyList<CardData> _all;
    private Queue<CardData> _current;
}