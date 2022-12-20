using System.Collections.Generic;
using System.Linq;

namespace DaresGameBot.Game.Data;

internal sealed class Deck<T> where T : Card
{
    public readonly string Tag;

    public bool Discarded;

    public Deck(string tag) => Tag = tag;

    public List<T> Cards { private get; init; } = new();

    public bool IsOkayFor(ushort playersAmount) => !Discarded && Cards.Any(c => c.IsOkayFor(playersAmount));

    public static Deck<T> GetShuffledCopy(Deck<T> deck, Shuffler shuffler) => deck.GetShuffledCopy(shuffler);

    public T? DrawFor(ushort playersAmount)
    {
        T? card = Cards.FirstOrDefault(c => c.IsOkayFor(playersAmount));
        if (card is null)
        {
            return null;
        }
        card.Discarded = true;
        return card;
    }

    private Deck<T> GetShuffledCopy(Shuffler shuffler)
    {
        List<T> cards = new(Cards);
        foreach (T card in Cards)
        {
            card.Discarded = false;
        }
        return new Deck<T>(Tag) { Cards = shuffler.Shuffle(cards) };
    }
}
