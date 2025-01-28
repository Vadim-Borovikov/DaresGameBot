using System.Collections.Generic;
using DaresGameBot.Game.Data;
using DaresGameBot.Game.Matchmaking.ActionCheck;

namespace DaresGameBot.Game.Decks;

internal sealed class DecksProvider
{
    public DecksProvider(IReadOnlyList<ActionData> actionDatas, IReadOnlyList<QuestionData> questionDatas)
    {
        Dictionary<ushort, ActionData> actionsDict = new();
        for (ushort i = 0; i < actionDatas.Count; i++)
        {
            actionsDict[i] = actionDatas[i];
        }
        _actionDatas = actionsDict;

        _questionDatas = questionDatas;
    }

    public QuestionDeck GetQuestionDeck() => new(_questionDatas);
    public ActionDeck GetActionDeck(IActionChecker checker) => new(_actionDatas, checker);

    private readonly Dictionary<ushort, ActionData> _actionDatas;
    private readonly IReadOnlyList<QuestionData> _questionDatas;
}