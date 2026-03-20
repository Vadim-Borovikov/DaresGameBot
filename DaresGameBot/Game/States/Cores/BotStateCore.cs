using System.Collections.Generic;
using DaresGameBot.Configs;

namespace DaresGameBot.Game.States.Cores;

internal sealed class BotStateCore
{
    public readonly Dictionary<string, Option> ActionOptions;
    public readonly ushort? QuestionPoints;
    public readonly string ActionsVersion;
    public readonly string QuestionsVersion;

    public SheetInfo? SheetInfo;

    public BotStateCore(Dictionary<string, Option> actionOptions, ushort? questionPoints, string actionsVersion,
        string questionsVersion)
    {
        ActionOptions = actionOptions;
        ActionsVersion = actionsVersion;
        QuestionsVersion = questionsVersion;
        QuestionPoints = questionPoints;
    }
}