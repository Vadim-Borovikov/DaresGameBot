using System.Collections.Generic;
using System.Linq;

namespace DaresGameBot.Game.Data
{
    internal sealed class Deck<T> where T : Card
    {
        public string Tag { internal get; set; }
        public List<T> Cards { private get; set; } = new List<T>();

        public bool Empty => Cards.Count == 0;

        public void Add(IEnumerable<T> cards) { Cards.AddRange(cards); }

        public static Deck<T> GetShuffledCopy(Deck<T> deck) => deck.GetShuffledCopy();

        public T Draw()
        {
            T card = Cards[0];
            Cards.RemoveAt(0);
            return card;
        }

        private Deck<T> GetShuffledCopy()
        {
            var cards = new List<T>(Cards);
            return new Deck<T>
            {
                Tag = Tag,
                Cards = cards.Shuffle().ToList()
            };
        }
    }
}