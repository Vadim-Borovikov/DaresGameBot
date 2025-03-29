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
        if (set.Count < size)
        {
            yield break;
        }

        if (set.Count == size)
        {
            yield return set;
            yield break;
        }

        Dictionary<int, List<List<T>>> previousSubsetsByMaxIndex = ChopToSubsets(set);

        for (uint s = 2; s <= size; s++)
        {
            Dictionary<int, List<List<T>>> currentSubsets = new();

            foreach (int previousIndex in previousSubsetsByMaxIndex.Keys)
            {
                for (int i = previousIndex + 1; i < set.Count; i++)
                {
                    if (!currentSubsets.ContainsKey(i))
                    {
                        currentSubsets[i] = new List<List<T>>();
                    }

                    foreach (List<T> previous in previousSubsetsByMaxIndex[previousIndex])
                    {
                        List<T> current = Create(previous, set[i]);
                        if (s == size)
                        {
                            yield return current;
                        }
                        else
                        {
                            currentSubsets[i].Add(current);
                        }
                    }
                }
            }

            previousSubsetsByMaxIndex = currentSubsets;
        }
    }

    private static Dictionary<int, List<List<T>>> ChopToSubsets<T>(IList<T> set)
    {
        Dictionary<int, List<List<T>>> dict = new();
        for (int i = 0; i < set.Count; i++)
        {
            dict[i] = new List<List<T>>
            {
                new() { set[i] }
            };
        }
        return dict;
    }

    private static List<T> Create<T>(IEnumerable<T> subset, T element) => new(subset) { element };
}