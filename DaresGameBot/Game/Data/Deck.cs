using System;
using System.Collections.Generic;
using System.Linq;

namespace DaresGameBot.Game.Data;

internal sealed class Deck<T> where T : Card
{
    public readonly string Tag;

    public Deck(string tag, IReadOnlyList<T> allCards, IList<ushort> indexes)
    {
        Tag = tag;
        _allCards = allCards;
        _indexes = indexes;
    }

    public bool IsOkayFor(int playersAmount) => _indexes.Any(i => _allCards[i].IsOkayFor(playersAmount));

    public T? DrawFor(int playersAmount)
    {
        List<ushort> options = _indexes.Where(i => _allCards[i].IsOkayFor(playersAmount)).ToList();
        if (options.Count == 0)
        {
            return null;
        }

        int optionIndex = _random.Next(options.Count);
        ushort option = options[optionIndex];
        _indexes.Remove(option);
        return _allCards[option];
    }

    private readonly IList<ushort> _indexes;
    private readonly IReadOnlyList<T> _allCards;
    private readonly Random _random = new();
}