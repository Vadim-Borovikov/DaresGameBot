using System.Collections.Generic;
using DaresGameBot.Game.Data.Cards;
using DaresGameBot.Game.Data.Decks;
using DaresGameBot.Game.Matchmaking.ActionCheck;

namespace DaresGameBot.Game;

internal sealed class DecksProvider
{
    public DecksProvider(IReadOnlyList<Action> actions, IReadOnlyList<Question> questions)
    {
        Dictionary<ushort, Action> actionsDict = new();
        for (ushort i = 0; i < actions.Count; i++)
        {
            actionsDict[i] = actions[i];
        }
        _actions = actionsDict;

        _questions = questions;
    }

    public QuestionDeck GetQuestionDeck() => new(_questions);
    public ActionDeck GetActionDeck(IActionChecker checker) => new(_actions, checker);

    private readonly Dictionary<ushort, Action> _actions;
    private readonly IReadOnlyList<Question> _questions;
}