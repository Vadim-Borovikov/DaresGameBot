using System.Collections.Generic;

namespace DaresGame
{
    public class Deck
    {
        internal readonly string Tag;
        internal bool Empty => _cards.Count == 0;

        private Queue<Card> _cards;

        public Deck(string tag, IEnumerable<Card> cards)
        {
            Tag = tag;
            _cards = new Queue<Card>(cards);
        }

        internal Deck Copy() => new Deck(Tag, _cards);

        internal void Shuffle() => _cards = _cards.ToShuffeledQueue();

        internal Card Draw() => _cards.Dequeue();
    }
}