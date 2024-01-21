using System.Collections.Generic;

namespace DaresGameBot.Game.Data;

internal sealed class Deck<T> where T : Card
{
    public readonly string Tag;

    public Deck(string tag, IReadOnlyList<T> allCards, List<int> indexes)
    {
        Tag = tag;
        _allCards = allCards;
        _indexes = indexes;
    }

    public Turn? TryGetTurn(Player player, ICardChecker<T> checker)
    {
        foreach (int i in _indexes)
        {
            T card = _allCards[i];
            Turn? turn = checker.TryGetTurn(player, card);
            if (turn is not null)
            {
                _indexes.Remove(i);
                return turn;
            }
        }

        return null;
    }

    private readonly List<int> _indexes;
    private readonly IReadOnlyList<T> _allCards;
}