// ReSharper disable MemberCanBeInternal
using System.Collections.Generic;

namespace DaresGame.Logic
{
    public class Deck
    {
        public string Tag { get; set; }
        public List<Card> Cards { get; set; }

        internal bool Empty => Cards.Count == 0;

        internal void Add(IEnumerable<Card> cards) { Cards.AddRange(cards); }

        internal void Shuffle() => Cards.Shuffle();

        internal static Deck Copy(Deck deck) => deck.Copy();

        internal Card Draw()
        {
            Card card = Cards[0];
            Cards.RemoveAt(0);
            return card;
        }

        private Deck Copy() => new Deck { Tag = Tag, Cards = new List<Card>(Cards) };
    }
}