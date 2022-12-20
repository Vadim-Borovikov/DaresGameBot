using System;
using System.Collections.Generic;
using System.Linq;
using GryphonUtilities.Extensions;

namespace DaresGameBot.Game.Data;

internal sealed class Game
{
    public ushort PlayersAmount;
    public float ChoiceChance;

    public string Players => $"Игроков: {PlayersAmount}";
    public string Chance => $"Шанс на 🤩: {ChoiceChance:P0}";

    public Game(ushort playersAmount, float choiceChance, IEnumerable<Deck<CardAction>> actionDecks,
        Deck<Card> questionsDeck)
    {
        PlayersAmount = playersAmount;
        ChoiceChance = choiceChance;

        _actionDecks =
            new List<Deck<CardAction>>(actionDecks.Select(d => Deck<CardAction>.GetShuffledCopy(d, _shuffler)));
        _questionsDeckFull = questionsDeck;
        _questionsDeckCurrent = new Deck<Card>(_questionsDeckFull.Tag);
    }

    public bool IsActive() => _actionDecks.Any(d => d.IsOkayFor(PlayersAmount));

    public Turn? DrawAction()
    {
        CardAction? card = DrawActionCard(out string deckTag);
        return card is null ? null : CreateActionTurn(card, deckTag);
    }

    public Turn DrawQuestion()
    {
        Card? card = _questionsDeckCurrent.DrawFor(PlayersAmount);
        if (card is null)
        {
            _questionsDeckCurrent = Deck<Card>.GetShuffledCopy(_questionsDeckFull, _shuffler);
            card = _questionsDeckCurrent.DrawFor(PlayersAmount).GetValue();
        }
        return CreateQuestionTurn(card, _questionsDeckCurrent.Tag);
    }

    private CardAction? DrawActionCard(out string deckTag)
    {
        foreach (Deck<CardAction> deck in _actionDecks)
        {
            if (deck.IsOkayFor(PlayersAmount))
            {
                deckTag = deck.Tag;
                return deck.DrawFor(PlayersAmount).GetValue("There should be cards in this deck");
            }

            deck.Discarded = true;
        }

        deckTag = "";
        return null;
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

    private readonly List<Deck<CardAction>> _actionDecks;
    private readonly Deck<Card> _questionsDeckFull;

    private Deck<Card> _questionsDeckCurrent;

    private readonly Shuffler _shuffler = new();
    private readonly Random _random = new();
}
