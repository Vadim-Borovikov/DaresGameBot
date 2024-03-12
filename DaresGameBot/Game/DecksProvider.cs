using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Game.Data.Cards;
using DaresGameBot.Game.Data.Decks;
using DaresGameBot.Game.Matchmaking.ActionCheck;

namespace DaresGameBot.Game;

internal sealed class DecksProvider
{
    public DecksProvider(IReadOnlyList<CardAction> actions, IReadOnlyList<Card> questions)
    {
        for (ushort i = 0; i < actions.Count; i++)
        {
            actions[i].Id = i;
        }

        _actions = actions;
        _questions = questions;
    }

    public QuestionDeck GetQuestionDeck() => new(_questions);

    public Queue<ActionDeck> GetActionDecks(IActionChecker checker)
    {
        IEnumerable<ActionDeck> decks = _actions.GroupBy(c => c.Tag).Select(g => new ActionDeck(g, checker));
        return new Queue<ActionDeck>(decks);
    }

    private readonly IReadOnlyList<CardAction> _actions;
    private readonly IReadOnlyList<Card> _questions;
}