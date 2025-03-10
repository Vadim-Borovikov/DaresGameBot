using System.Collections.Generic;
using DaresGameBot.Game.Data;

namespace DaresGameBot;

internal sealed class SheetInfo
{
    public readonly Dictionary<ushort, ActionData> Actions;
    public readonly Dictionary<ushort, CardData> Questions;

    public SheetInfo(Dictionary<ushort, ActionData> actions, Dictionary<ushort, CardData> questions)
    {
        Actions = actions;
        Questions = questions;
    }
}