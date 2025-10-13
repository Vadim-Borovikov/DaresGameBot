using System.Collections.Generic;
using DaresGameBot.Game.Data;

namespace DaresGameBot;

internal sealed class SheetInfo
{
    public readonly Dictionary<ushort, ActionData> Actions;
    public readonly Dictionary<ushort, QuestionData> Questions;

    public SheetInfo(Dictionary<ushort, ActionData> actions, Dictionary<ushort, QuestionData> questions)
    {
        Actions = actions;
        Questions = questions;
    }
}