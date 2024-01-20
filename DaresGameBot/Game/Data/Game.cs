using System;
using System.Collections.Generic;
using System.Linq;

namespace DaresGameBot.Game.Data;

internal sealed class Game
{
    public byte PlayersAmount;
    public decimal ChoiceChance;

    public Game(byte playersAmount, decimal choiceChance, IList<Deck<CardAction>> actionDecks,
        Deck<Card> questionsDeck)
    {
        PlayersAmount = playersAmount;
        ChoiceChance = choiceChance;
        _actionDecks = actionDecks;
        _questionsDeck = questionsDeck;
    }

    public bool IsActive() => _actionDecks.Any(d => d.IsOkayFor(PlayersAmount));

    public Turn? DrawAction()
    {
        CardAction? card = DrawActionCard();
        return card is null ? null : CreateActionTurn(card);
    }

    public void SetQuestions(Deck<Card> questionsDeck) => _questionsDeck = questionsDeck;

    public Turn? DrawQuestion()
    {
        Card? card = _questionsDeck.DrawFor(PlayersAmount);
        return card is null ? null : CreateQuestionTurn(card, _questionsDeck.Tag);
    }

    private CardAction? DrawActionCard()
    {
        while (_actionDecks.Any())
        {
            CardAction? card = _actionDecks.First().DrawFor(PlayersAmount);
            if (card is not null)
            {
                return card;
            }

            _actionDecks.RemoveAt(0);
        }

        return null;
    }

    private Turn CreateActionTurn(CardAction card)
    {
        byte[] players = Enumerable.Range(1, PlayersAmount - 1).Select(i => (byte) i).ToArray();
        _random.Shuffle(players);
        Queue<byte> partnersQueue = new(players);

        List<Partner> partners = new(card.PartnersToAssign);
        for (byte i = 0; i < card.PartnersToAssign; ++i)
        {
            bool byChoice = (decimal)_random.NextSingle() < ChoiceChance;
            Partner partner = byChoice ? new Partner() : new Partner(partnersQueue.Dequeue());
            partners.Add(partner);
        }
        partners.Sort();

        return new Turn($"{card.Tag} {card.Description}", partners);
    }

    private static Turn CreateQuestionTurn(Card card, string deckTag) => new($"{deckTag} {card.Description}");

    private readonly Random _random = new();
    private readonly IList<Deck<CardAction>> _actionDecks;
    private Deck<Card> _questionsDeck;
}
