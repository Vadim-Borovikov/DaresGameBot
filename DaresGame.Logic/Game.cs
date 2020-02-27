﻿using System.Collections.Generic;
using System.Linq;

namespace DaresGame.Logic
{
    public class Game
    {
        public int PlayersAmount;
        public float ChoiceChance;

        public string Players => $"Игроков: {PlayersAmount}";
        public string Chance => $"Шанс на 🤩: {ChoiceChance:P0}";

        public bool Empty => _decks.Count == 0;

        public Game(int playersAmount, float choiceChance, IEnumerable<Deck> decks)
        {
            PlayersAmount = playersAmount;
            ChoiceChance = choiceChance;

            _decks = new Queue<Deck>();
            foreach (Deck deck in decks.Select(Deck.Copy))
            {
                deck.Shuffle();
                _decks.Enqueue(deck);
            }
        }

        public Turn Draw()
        {
            Card card = DrawCard(out string deckTag);
            return CreateTurn(card, deckTag);
        }

        private Card DrawCard(out string deckTag)
        {
            while (true)
            {
                if (Empty)
                {
                    deckTag = null;
                    return null;
                }

                Deck current = _decks.Peek();
                deckTag = current.Tag;

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

        private Turn CreateTurn(Card card, string deckTag)
        {
            Queue<int> partnersQueue = Enumerable.Range(1, PlayersAmount - 1).ToShuffeledQueue();

            var partners = new List<Partner>(card.PartnersToAssign);
            for (int i = 0; i < card.PartnersToAssign; ++i)
            {
                bool byChoice = Utils.Random.NextDouble() < ChoiceChance;
                Partner partner = byChoice ? new Partner() : new Partner(partnersQueue.Dequeue());
                partners.Add(partner);
            }
            partners.Sort();

            return new Turn($"{deckTag} {card.Description}", partners);
        }

        private readonly Queue<Deck> _decks;
    }
}
