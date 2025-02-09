using System;
using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Helpers;

namespace DaresGameBot.Game;

internal sealed class Deck<T>
{
    public Deck(IReadOnlyList<T> cards)
    {
        for (ushort i = 0; i < cards.Count; ++i)
        {
            _cards[i] = cards[i];
            _uses[i] = 0;
        }
    }

    public T GetCard(ushort id) => _cards[id];

    public ushort? GetRandomId(Func<T, bool>? predicate = null)
    {
        List<ushort> ids = _cards.Keys.Where(id => predicate?.Invoke(_cards[id]) ?? true).ToList();

        if (!ids.Any())
        {
            return null;
        }

        uint minUses = ids.Min(id => _uses[id]);
        return RandomHelper.SelectItem(ids.Where(id => _uses[id] == minUses).ToList());
    }

    public void Mark(ushort id) => ++_uses[id];

    private readonly Dictionary<ushort, T> _cards = new();
    private readonly Dictionary<ushort, uint> _uses = new();
}