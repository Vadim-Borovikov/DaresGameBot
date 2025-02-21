using System;
using DaresGameBot.Operations.Data.GameButtons;

namespace DaresGameBot.Helpers;

internal static class ObjectExtensions
{
    public static ushort? ToUshort(this object? o)
    {
        if (o is ushort u)
        {
            return u;
        }
        return ushort.TryParse(o?.ToString(), out u) ? u : null;
    }

    public static ConfirmEndData.ActionAfterGameEnds? ToActionAfterGameEnds(this object? o)
    {
        if (o is ConfirmEndData.ActionAfterGameEnds a)
        {
            return a;
        }
        return Enum.TryParse(o?.ToString(), out a) ? a : null;
    }

    public static Context.Game.State? ToState(this object? o)
    {
        if (o is Context.Game.State s)
        {
            return s;
        }
        return Enum.TryParse(o?.ToString(), out s) ? s : null;
    }
}