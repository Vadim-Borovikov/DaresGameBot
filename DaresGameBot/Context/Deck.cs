using System;
using System.Collections.Generic;
using System.Linq;
using AbstractBot;
using DaresGameBot.Helpers;

namespace DaresGameBot.Context;

internal sealed class Deck<T> : IContext<Deck<T>, Dictionary<ushort, uint>, Dictionary<ushort, T>>
{
    public Deck(Dictionary<ushort, T> cards, Dictionary<ushort, uint>? uses = null)
    {
        _cards = cards;
        _uses = uses ?? new Dictionary<ushort, uint>();
    }

    public T GetCard(ushort id) => _cards[id];

    public ushort? GetRandomId(Func<T, bool>? predicate = null)
    {
        List<ushort> ids = _cards.Keys.Where(id => predicate?.Invoke(_cards[id]) ?? true).ToList();

        if (!ids.Any())
        {
            return null;
        }

        uint minUses = ids.Min(id => _uses.GetValueOrDefault(id));
        return RandomHelper.SelectItem(ids.Where(id => _uses.GetValueOrDefault(id) == minUses).ToList());
    }

    public void Mark(ushort id) => _uses.CreateOrAdd(id, 1);

    public Dictionary<ushort, uint> Save() => _uses;

    public static Deck<T>? Load(Dictionary<ushort, uint> data, Dictionary<ushort, T>? meta)
    {
        return meta is null ? null : new Deck<T>(meta, data);
    }

    private readonly Dictionary<ushort, T> _cards;
    private readonly Dictionary<ushort, uint> _uses;
}