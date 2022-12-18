using System.Collections.Generic;

namespace DaresGameBot.Game.Data;

internal sealed class Deck<T> where T : Card
{
    internal readonly string Tag;

    internal Deck(string tag) => Tag = tag;

    public List<T> Cards { private get; init; } = new();

    public bool Empty => Cards.Count == 0;

    public void Add(IEnumerable<T> cards) => Cards.AddRange(cards);

    public static Deck<T> GetShuffledCopy(Deck<T> deck, Shuffler shuffler) => deck.GetShuffledCopy(shuffler);

    public T Draw()
    {
        T card = Cards[0];
        Cards.RemoveAt(0);
        return card;
    }

    private Deck<T> GetShuffledCopy(Shuffler shuffler)
    {
        List<T> cards = new(Cards);
        return new Deck<T>(Tag) { Cards = shuffler.Shuffle(cards) };
    }
}
