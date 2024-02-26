using System.Collections.Generic;

namespace DaresGameBot.Helpers;

internal static class ListHelper
{
    public static IEnumerable<(T, T)> EnumeratePairs<T>(IReadOnlyList<T> items)
    {
        for (int i = 0; i < items.Count; i++)
        {
            for (int j = i + 1; j < items.Count; j++)
            {
                yield return (items[i], items[j]);
            }
        }
    }
}