using System.Collections.Generic;
using System.Linq;

namespace DaresGameBot.Logic
{
    public sealed class Deck
    {
        public string Tag { internal get; set; }
        public List<Card> Cards { private get; set; }

        internal bool Empty => Cards.Count == 0;

        internal void Add(IEnumerable<Card> cards) { Cards.AddRange(cards); }

        internal static Deck GetShuffledCopy(Deck deck) => deck.GetShuffledCopy();

        internal Card Draw()
        {
            Card card = Cards[0];
            Cards.RemoveAt(0);
            return card;
        }

        private Deck GetShuffledCopy()
        {
            var cards = new List<Card>(Cards);
            return new Deck
            {
                Tag = Tag,
                Cards = cards.Shuffle().ToList()
            };
        }
    }
}