﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace DaresGameBot.Game.Data;

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

        _actionDecks =
            new Queue<Deck<CardAction>>(actionDecks.Select(d => Deck<CardAction>.GetShuffledCopy(d, _shuffler)));
        _questionsDeckFull = questionsDeck;
        _questionsDeckCurrent = new Deck<Card>(_questionsDeckFull.Tag);
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
            _questionsDeckCurrent = Deck<Card>.GetShuffledCopy(_questionsDeckFull, _shuffler);
        }

        Card card = _questionsDeckCurrent.Draw();

        return CreateQuestionTurn(card, _questionsDeckCurrent.Tag);
    }

    private CardAction DrawActionCard(out string deckTag)
    {
        while (true)
        {
            Deck<CardAction> current = _actionDecks.Peek();
            deckTag = current.Tag;

            Queue<CardAction> crowdCards = new();
            CardAction card = DrawAction(current, crowdCards);

            if (current.Empty)
            {
                _actionDecks.Dequeue();
            }

            current.Add(crowdCards);
            return card;
        }
    }

    private CardAction DrawAction(Deck<CardAction> deck, Queue<CardAction> crowdCards)
    {
        while (true)
        {
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
        IEnumerable<ushort> players = Enumerable.Range(1, PlayersAmount - 1).Select(i => (ushort) i);
        List<ushort> shuffled = _shuffler.Shuffle(players);
        Queue<ushort> partnersQueue = new(shuffled);

        List<Partner> partners = new(card.PartnersToAssign);
        for (ushort i = 0; i < card.PartnersToAssign; ++i)
        {
            bool byChoice = _random.NextDouble() < ChoiceChance;
            Partner partner = byChoice ? new Partner() : new Partner(partnersQueue.Dequeue());
            partners.Add(partner);
        }
        partners.Sort();

        return new Turn($"{deckTag} {card.Description}", partners);
    }

    private static Turn CreateQuestionTurn(Card card, string deckTag) => new($"{deckTag} {card.Description}");

    private readonly Queue<Deck<CardAction>> _actionDecks;
    private readonly Deck<Card> _questionsDeckFull;

    private Deck<Card> _questionsDeckCurrent;

    private readonly Shuffler _shuffler = new();
    private readonly Random _random = new();
}
