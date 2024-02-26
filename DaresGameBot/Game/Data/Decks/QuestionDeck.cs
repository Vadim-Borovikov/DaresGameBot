using System.Collections.Generic;
using DaresGameBot.Game.Data.Cards;
using DaresGameBot.Helpers;

namespace DaresGameBot.Game.Data.Decks;

internal sealed class QuestionDeck
{
    public QuestionDeck(IReadOnlyList<Card> source)
    {
        _source = source;
        _current = new Queue<Card>();
    }

    public Card Draw()
    {
        if (_current.Count == 0)
        {
            IEnumerable<Card> items = RandomHelper.Shuffle(_source);
            _current = new Queue<Card>(items);
        }

        return _current.Dequeue();
    }

    private readonly IReadOnlyList<Card> _source;
    private Queue<Card> _current;
}