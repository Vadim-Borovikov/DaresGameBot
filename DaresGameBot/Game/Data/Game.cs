using System.Collections.Generic;
using System.Linq;

namespace DaresGameBot.Game.Data
{
    internal sealed class Game
    {
        public ushort PlayersAmount;
        public float ChoiceChance;
        public ushort RejectsAmount;

        public string Players => $"Игроков: {PlayersAmount}";
        public string Chance => $"Шанс на 🤩: {ChoiceChance:P0}";
        public string Rejects => $"Отказов в ходе: {RejectsAmount}";

        public bool Empty => _decks.Count == 0;

        public Game(ushort playersAmount, float choiceChance, ushort rejectsAmount, IEnumerable<Deck> decks)
        {
            PlayersAmount = playersAmount;
            ChoiceChance = choiceChance;
            RejectsAmount = rejectsAmount;

            _decks = new Queue<Deck>(decks.Select(Deck.GetShuffledCopy));
        }

        public Card Draw()
        {
            while (true)
            {
                if (Empty)
                {
                    return null;
                }

                Deck current = _decks.Peek();

                var crowdCards = new Queue<Card>();
                Card card = Draw(current, crowdCards);

                if (current.Empty)
                {
                    _decks.Dequeue();
                    if (card == null)
                    {
                        continue;
                    }
                }

                current.Add(crowdCards);
                return card;
            }
        }

        private Card Draw(Deck deck, Queue<Card> crowdCards)
        {
            while (true)
            {
                if (deck.Empty)
                {
                    return null;
                }

                Card next = deck.Draw();
                if (next.Players <= PlayersAmount)
                {
                    return next;
                }

                crowdCards.Enqueue(next);
            }
        }

        public Turn CreateTurn(Card card)
        {
            List<Partner> partners = RollPartners(card.PartnersToAssign);
            return new Turn(card, partners, RejectsAmount);
        }

        public void Reroll(Turn turn)
        {
            List<Partner> partners = RollPartners(turn.Card.PartnersToAssign, turn.MarkedPartners.ToList());
            turn.Reject(partners);
        }

        private List<Partner> RollPartners(int amount, ICollection<ushort> leastPossible = null)
        {
            IEnumerable<ushort> range = Enumerable.Range(1, PlayersAmount - 1)
                                                  .Select(i => (ushort)i);
            if (leastPossible != null)
            {
                range = range.Where(u => !leastPossible.Contains(u));
            }
            Queue<ushort> partnersQueue = range.ToShuffeledQueue();

            if (leastPossible != null)
            {
                partnersQueue.AddRange(leastPossible.Where(u => u < PlayersAmount).ToList().Shuffle());
            }

            var partners = new List<Partner>(amount);
            for (ushort i = 0; i < amount; ++i)
            {
                bool byChoice = Utils.Random.NextDouble() < ChoiceChance;
                Partner partner = byChoice ? new Partner() : new Partner(partnersQueue.Dequeue());
                partners.Add(partner);
            }
            partners.Sort();
            return partners;
        }

        private readonly Queue<Deck> _decks;
    }
}
