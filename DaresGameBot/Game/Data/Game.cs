using System;
using System.Collections.Generic;
using System.Linq;
using AbstractBot;

namespace DaresGameBot.Game.Data;

internal sealed class Game : Context
{
    public IEnumerable<string> PlayerNames => _players.Select(p => p.Name);
    public decimal ChoiceChance;

    public Game(IEnumerable<string> playerNames, decimal choiceChance, IList<Deck<CardAction>> actionDecks,
        Deck<Card> questionsDeck)
    {
        ChoiceChance = choiceChance;
        _actionDecks = actionDecks;
        _questionsDeck = questionsDeck;

        UpdatePlayers(playerNames);
    }

    public bool IsActive() => _actionDecks.Any(d => d.IsOkayFor(_players.Count));

    public Turn? DrawAction()
    {
        CardAction? card = DrawActionCard();
        return card is null ? null : CreateActionTurn(card);
    }

    public void SetQuestions(Deck<Card> questionsDeck) => _questionsDeck = questionsDeck;

    public Turn? DrawQuestion()
    {
        Card? card = _questionsDeck.DrawFor(_players.Count);
        return card is null ? null : CreateQuestionTurn(card, _questionsDeck.Tag);
    }

    public void UpdatePlayers(IEnumerable<string> playerNames)
    {
        _players.Clear();
        _players.AddRange(playerNames.Select(n => new Player(n)));
        _currentPlayerIndex = 0;
    }

    private CardAction? DrawActionCard()
    {
        while (_actionDecks.Any())
        {
            CardAction? card = _actionDecks.First().DrawFor(_players.Count);
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
        List<Partner> partners = new();
        if (card.PartnersToAssign > 0)
        {
            Player[] choices = _players.Where((_, i) => i != _currentPlayerIndex).ToArray();
            _random.Shuffle(choices);
            for (byte i = 0; i < card.PartnersToAssign; ++i)
            {
                bool byChoice = (decimal)_random.NextSingle() < ChoiceChance;
                partners.Add(new Partner(byChoice ? null : choices[i]));
            }
            partners.Sort();
        }

        Player player = _players[_currentPlayerIndex];
        SwitchPlayer();
        return new Turn($"{card.Tag} {card.Description}", player, partners);
    }

    private static Turn CreateQuestionTurn(Card card, string deckTag) => new($"{deckTag} {card.Description}");

    private void SwitchPlayer() => _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;

    private readonly Random _random = new();
    private readonly IList<Deck<CardAction>> _actionDecks;
    private Deck<Card> _questionsDeck;
    private readonly List<Player> _players = new();
    private int _currentPlayerIndex;
}
