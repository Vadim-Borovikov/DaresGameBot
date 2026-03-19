using System;

namespace DaresGameBot.Utilities.Extensions;

internal static class ObjectExtensions
{
    public static T? ToEnum<T>(this object? o) where T : struct, Enum
    {
        if (o is T s)
        {
            return s;
        }
        return Enum.TryParse(o?.ToString(), out s) ? s : null;
    }
}