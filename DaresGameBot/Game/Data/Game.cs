using System.Collections.Generic;
using System.Linq;

namespace DaresGameBot.Game.Data
{
    internal sealed class Game
    {
        public ushort PlayersAmount;
        public float ChoiceChance;

        public string Players => $"Игроков: {PlayersAmount}";
        public string Chance => $"Шанс на 🤩: {ChoiceChance:P0}";

        public bool Empty => _actionDecks.Count == 0;

        public Game(ushort playersAmount, float choiceChance, IEnumerable<Deck<CardAction>> actionDecks,
            Deck<Card> questionsDeck)
        {
            PlayersAmount = playersAmount;
            ChoiceChance = choiceChance;

            _actionDecks = new Queue<Deck<CardAction>>(actionDecks.Select(Deck<CardAction>.GetShuffledCopy));
            _questionsDeckFull = questionsDeck;
            _questionsDeckCurrent = new Deck<Card>();
        }

        public Turn DrawAction()
        {
            CardAction card = DrawActionCard(out string deckTag);
            return CreateActionTurn(card, deckTag);
        }

        public Turn DrawQuestion()
        {
            if (_questionsDeckCurrent.Empty)
            {
                _questionsDeckCurrent = Deck<Card>.GetShuffledCopy(_questionsDeckFull);
            }

            Card card = _questionsDeckCurrent.Draw();

            return CreateQuestionTurn(card, _questionsDeckCurrent.Tag);
        }

        private CardAction DrawActionCard(out string deckTag)
        {
            while (true)
            {
                if (Empty)
                {
                    deckTag = null;
                    return null;
                }

                Deck<CardAction> current = _actionDecks.Peek();
                deckTag = current.Tag;

                var crowdCards = new Queue<CardAction>();
                CardAction card = DrawAction(current, crowdCards);

                if (current.Empty)
                {
                    _actionDecks.Dequeue();
                    if (card == null)
                    {
                        continue;
                    }
                }

                current.Add(crowdCards);
                return card;
            }
        }

        private CardAction DrawAction(Deck<CardAction> deck, Queue<CardAction> crowdCards)
        {
            while (true)
            {
                if (deck.Empty)
                {
                    return null;
                }

                CardAction next = deck.Draw();
                if (next.Players <= PlayersAmount)
                {
                    return next;
                }

                crowdCards.Enqueue(next);
            }
        }

        private Turn CreateActionTurn(CardAction card, string deckTag)
        {
            Queue<ushort> partnersQueue = Enumerable.Range(1, PlayersAmount - 1)
                                                    .Select(i => (ushort) i)
                                                    .ToShuffeledQueue();

            var partners = new List<Partner>(card.PartnersToAssign);
            for (ushort i = 0; i < card.PartnersToAssign; ++i)
            {
                bool byChoice = Utils.Random.NextDouble() < ChoiceChance;
                Partner partner = byChoice ? new Partner() : new Partner(partnersQueue.Dequeue());
                partners.Add(partner);
            }
            partners.Sort();

            return new Turn($"{deckTag} {card.Description}", partners);
        }

        private static Turn CreateQuestionTurn(Card card, string deckTag) => new Turn($"{deckTag} {card.Description}");

        private readonly Queue<Deck<CardAction>> _actionDecks;
        private readonly Deck<Card> _questionsDeckFull;

        private Deck<Card> _questionsDeckCurrent;
    }
}
