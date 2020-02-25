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
            foreach (Deck deck in _settings.Decks.Select(d => d.Copy()))
            {
                deck.Shuffle();
                _decks.Enqueue(deck);
            }
        }

        public void UpdateSettings(Settings settings)
        {
            _settings = settings;
        }

        public Turn Draw()
        {
            if (Empty)
            {
                return null;
            }

            Deck current = _decks.Peek();
            var turn = new Turn();
            Card card = current.Draw();
            turn.Text = $"{current.Tag} {card.Description}";
            if (current.Empty)
            {
                _decks.Dequeue();
            }

            turn.Partners = new List<Partner>(card.PartnersAmount);
            Queue<int> partners = Enumerable.Range(1, _settings.PlayersAmount - 1).ToShuffeledQueue();
            for (int i = 0; i < card.PartnersAmount; ++i)
            {
                bool byChoice = Utils.Random.NextDouble() < _settings.ChoiceChance;
                Partner partner = byChoice ? new Partner(true) : new Partner(partners.Dequeue());
                turn.Partners.Add(partner);
            }
            turn.Partners.Sort();

            return turn;
        }
    }
}
