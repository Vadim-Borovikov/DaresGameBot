using System.Collections.Generic;
using DaresGameBot.Game.Data;
using DaresGameBot.Game.Matchmaking.ActionCheck;

namespace DaresGameBot.Game.Decks;

internal sealed class DecksProvider
{
    public DecksProvider(IReadOnlyList<ActionData> actionDatas, IReadOnlyList<CardData> questionDatas)
    {
        _actionDatas = actionDatas;
        _questionDatas = questionDatas;
    }

    public QuestionDeck GetQuestionDeck() => new(_questionDatas);
    public ActionDeck GetActionDeck(IActionChecker checker) => new(_actionDatas, checker);

    private readonly IReadOnlyList<ActionData> _actionDatas;
    private readonly IReadOnlyList<CardData> _questionDatas;
}