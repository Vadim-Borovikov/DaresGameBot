using System.Collections.Generic;
using DaresGameBot.Game.Data;
using DaresGameBot.Helpers;

namespace DaresGameBot.Game.Decks;

internal sealed class QuestionDeck
{
    public QuestionDeck(IReadOnlyList<QuestionData> cards)
    {
        _all = cards;
        _current = new Queue<QuestionData>();
    }

    public QuestionData Draw()
    {
        if (_current.Count == 0)
        {
            IEnumerable<QuestionData> items = RandomHelper.Shuffle(_all);
            _current = new Queue<QuestionData>(items);
        }

        return _current.Dequeue();
    }

    private readonly IReadOnlyList<QuestionData> _all;
    private Queue<QuestionData> _current;
}