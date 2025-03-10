using System.Collections.Generic;

namespace DaresGameBot.Utilities;

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

    public static IEnumerable<IList<T>> EnumerateSubsets<T>(IList<T> set, uint size)
    {
        if (set.Count <= size)
        {
            if (set.Count == size)
            {
                yield return set;
            }
            yield break;
        }

        for (int i = 0; i < set.Count; i++)
        {
            List<T> subset = new(set);
            subset.RemoveAt(i);
            foreach (IList<T> subsetOfSubset in EnumerateSubsets(subset, size))
            {
                yield return subsetOfSubset;
            }
        }
    }
}