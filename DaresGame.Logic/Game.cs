using System.Collections.Generic;
using System.Linq;

namespace DaresGame.Logic
{
    public class Game
    {
        private readonly Queue<Deck> _decks;
        private Settings _settings;
        public bool Empty => _decks.Count == 0;

        public Game(Settings settings)
        {
            _settings = settings;

            _decks = new Queue<Deck>();
            foreach (Deck deck in _settings.Decks.Select(Deck.Copy))
            {
                deck.Shuffle();
                _decks.Enqueue(deck);
            }
        }

        public void UpdateSettings(Settings settings) { _settings = settings; }

        public Turn Draw()
        {
            Card card = DrawCard(out string deckTag);
            return CreateTurn(card, deckTag);
        }

        private Card DrawCard(out string deckTag)
        {
            if (Empty)
            {
                deckTag = null;
                return null;
            }

            Deck current = _decks.Peek();
            deckTag = current.Tag;
            Card card = current.Draw();

            if (current.Empty)
            {
                _decks.Dequeue();
            }

            return card;
        }

        private Turn CreateTurn(Card card, string deckTag)
        {
            Queue<int> partnersQueue = Enumerable.Range(1, _settings.PlayersAmount - 1).ToShuffeledQueue();

            var partners = new List<Partner>(card.PartnersAmount);
            for (int i = 0; i < card.PartnersAmount; ++i)
            {
                bool byChoice = Utils.Random.NextDouble() < _settings.ChoiceChance;
                Partner partner = byChoice ? new Partner() : new Partner(partnersQueue.Dequeue());
                partners.Add(partner);
            }
            partners.Sort();

            return new Turn($"{deckTag} {card.Description}", partners);
        }
    }
}
