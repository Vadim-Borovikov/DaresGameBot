using System;
using DaresGameBot.Operations.Data.GameButtons;

namespace DaresGameBot.Utilities.Extensions;

internal static class ObjectExtensions
{
    public static ConfirmEndData.ActionAfterGameEnds? ToActionAfterGameEnds(this object? o)
    {
        if (o is ConfirmEndData.ActionAfterGameEnds a)
        {
            return a;
        }
        return Enum.TryParse(o?.ToString(), out a) ? a : null;
    }

    public static Game.States.Game.State? ToState(this object? o)
    {
        if (o is Game.States.Game.State s)
        {
            return s;
        }
        return Enum.TryParse(o?.ToString(), out s) ? s : null;
    }
}