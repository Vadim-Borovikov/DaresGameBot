using DaresGameBot.Configs;
using System.Collections.Generic;
using DaresGameBot.Game.Data;

namespace DaresGameBot.Context.Meta;

public class MetaContext
{
    internal readonly Dictionary<string, Option> ActionOptions;
    internal readonly Dictionary<ushort, ActionData> Actions;
    internal readonly Dictionary<ushort, CardData> Questions;
    internal readonly string ActionsVersion;
    internal readonly string QuestionsVersion;

    internal MetaContext(Dictionary<string, Option> actionOptions, Dictionary<ushort, ActionData> actions,
        Dictionary<ushort, CardData> questions, string actionsVersion, string questionsVersion)
    {
        ActionOptions = actionOptions;
        Actions = actions;
        Questions = questions;
        ActionsVersion = actionsVersion;
        QuestionsVersion = questionsVersion;
    }
}