using System;
using System.Collections.Generic;
using System.Linq;
using AbstractBot;

namespace DaresGameBot.Game.Data;

internal sealed class Game : Context
{
    public IEnumerable<string> PlayerNames => _players.Select(p => p.Name);
    public bool Fresh;

    public Game(List<Player> players, IList<Deck<CardAction>> actionDecks,
        Deck<Card> questionsDeck)
    {
        Fresh = true;
        _actionDecks = actionDecks;
        _questionsDeck = questionsDeck;

        UpdatePlayers(players);
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

    public void UpdatePlayers(List<Player> players)
    {
        _players = players;
        _currentPlayerIndex = 0;
    }

    public void SwitchPlayer() => _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;

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
        Player player = _players[_currentPlayerIndex];
        List<Player> partners = new();

        if (card.PartnersToAssign > 0)
        {
            Player[] choices = _players.Where(p => Player.AreCompatable(p, player)).ToArray();
            _random.Shuffle(choices);
            partners.AddRange(choices.Take(card.PartnersToAssign));
        }

        return new Turn($"{card.Tag} {card.Description}", player, partners);
    }

    private Turn CreateQuestionTurn(Card card, string deckTag)
    {
        Player player = _players[_currentPlayerIndex];
        return new Turn($"{deckTag} {card.Description}", player);
    }

    private readonly Random _random = new();
    private readonly IList<Deck<CardAction>> _actionDecks;
    private Deck<Card> _questionsDeck;
    private List<Player> _players = new();
    private int _currentPlayerIndex;
}
