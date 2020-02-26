// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
using System.Collections.Generic;
using System.Linq;

namespace DaresGame.Logic
{
    public class Deck
    {
        public string Tag { get; set; }
        public List<Card> Cards
        {
            get => _cards.ToList();
            set => _cards = new Queue<Card>(value);
        }

        internal bool Empty => _cards.Count == 0;

        private Queue<Card> _cards = new Queue<Card>();

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
        private Deck Copy() => new Deck { Tag = Tag, _cards = _cards.ToQueue() };
    }
}