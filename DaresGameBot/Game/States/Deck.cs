using System;
using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Utilities;
using DaresGameBot.Utilities.Extensions;
using GryphonUtilities.Save;

namespace DaresGameBot.Game.States;

internal sealed class Deck<T> : IStateful<Dictionary<ushort, uint>>
{
    public Deck(Dictionary<ushort, T> cards) => _cards = cards;

    public T GetCard(ushort id) => _cards[id];

    public IEnumerable<IGrouping<uint, ushort>> GroupByUses(IEnumerable<ushort> ids)
    {
        return ids.GroupBy(_uses.GetValueOrDefault);
    }

    public IEnumerable<ushort> GetIds(Func<T, bool>? predicate = null)
    {
        return predicate is null ? _cards.Keys : _cards.Keys.Where(id => predicate.Invoke(_cards[id]));
    }

    public IEnumerable<ushort> FilterMinUses(ICollection<ushort>? ids = null)
    {
        ids ??= _cards.Keys;
        uint minUses = ids.Min(id => _uses.GetValueOrDefault(id));
        return ids.Where(id => _uses.GetValueOrDefault(id) == minUses);
    }

    public void Mark(ushort id) => _uses.CreateOrAdd(id, 1);

    public Dictionary<ushort, uint> Save() => _uses;

    public void LoadFrom(Dictionary<ushort, uint>? data)
    {
        if (data is null)
        {
            return;
        }

        _uses.Clear();
        _uses.AddAll(data);
    }

    private readonly Dictionary<ushort, T> _cards;
    private readonly Dictionary<ushort, uint> _uses = new();
}