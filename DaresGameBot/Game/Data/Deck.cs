using System;
using System.Collections.Generic;
using System.Linq;

namespace DaresGameBot.Game.Data;

internal sealed class Deck<T> where T : Card
{
    public readonly string Tag;

    public bool Discarded;

    public Deck(string tag) => Tag = tag;

    public IList<T> Cards { private get; init; } = Array.Empty<T>();

    public bool IsOkayFor(byte playersAmount) => !Discarded && Cards.Any(c => c.IsOkayFor(playersAmount));

    public T? DrawFor(byte playersAmount)
    {
        T? card = Cards.FirstOrDefault(c => c.IsOkayFor(playersAmount));
        if (card is null)
        {
            return null;
        }
        card.Discarded = true;
        return card;
    }

    public Deck<T> GetShuffledCopy()
    {
        T[] cards = Cards.ToArray();
        foreach (T card in Cards)
        {
            card.Discarded = false;
        }
        _random.Shuffle(cards);
        return new Deck<T>(Tag) { Cards = cards };
    }

    private readonly Random _random = new();
}
