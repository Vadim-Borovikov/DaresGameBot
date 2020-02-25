using System.Collections.Generic;

namespace DaresGame.Logic
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

        internal static Deck Copy(Deck deck) => deck.Copy();

        internal void Shuffle() => _cards = _cards.ToShuffeledQueue();

        internal Card Draw() => _cards.Dequeue();

        internal void Enqueue(IEnumerable<Card> cards)
        {
            foreach (Card card in cards)
            {
                _cards.Enqueue(card);
            }
        }

        private Deck Copy() => new Deck(Tag, _cards);
    }
}