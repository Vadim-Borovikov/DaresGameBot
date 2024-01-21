using System.Collections.Generic;

namespace DaresGameBot.Game.Data;

internal sealed class Deck<T> where T : Card
{
    public readonly string Tag;

    public Deck(string tag, IReadOnlyList<T> allCards, List<int> indices)
    {
        Tag = tag;
        _allCards = allCards;
        _indices = indices;
    }

    public Turn? TryGetTurn(Player player, ICardChecker<T> checker)
    {
        foreach (int i in _indices)
        {
            T card = _allCards[i];
            Turn? turn = checker.TryGetTurn(player, card);
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