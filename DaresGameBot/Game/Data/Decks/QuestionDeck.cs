using System.Collections.Generic;
using DaresGameBot.Game.Data.Cards;
using DaresGameBot.Helpers;

namespace DaresGameBot.Game.Data.Decks;

internal sealed class QuestionDeck
{
    public QuestionDeck(IReadOnlyList<Question> source)
    {
        _source = source;
        _current = new Queue<Question>();
    }

    public Question Draw()
    {
        if (_current.Count == 0)
        {
            IEnumerable<Question> items = RandomHelper.Shuffle(_source);
            _current = new Queue<Question>(items);
        }

        return _current.Dequeue();
    }

    private readonly IReadOnlyList<Question> _source;
    private Queue<Question> _current;
}