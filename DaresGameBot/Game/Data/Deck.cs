using System.Collections.Generic;
using System.Linq;

namespace DaresGameBot.Game.Data
{
    internal sealed class Deck
    {
        public string Tag { internal get; set; }
        public List<Card> Cards { private get; set; }

        public bool Empty => Cards.Count == 0;

        public void Add(IEnumerable<Card> cards) { Cards.AddRange(cards); }

        public static Deck GetShuffledCopy(Deck deck) => deck.GetShuffledCopy();

        public Card Draw()
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