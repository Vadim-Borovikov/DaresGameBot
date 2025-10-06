using System;
using System.Collections.Generic;
using System.Linq;

namespace DaresGameBot.Utilities;

internal static class RandomHelper
{
    public static T RandomItem<T>(this IEnumerable<T> source) => source.OrderByShuffled().First();

    public static T RandomItemWeighted<T>(this IDictionary<T, ushort> source)
    {
        int totalWeight = source.Values.Aggregate(0, (current, weight) => current + weight);
        if (totalWeight == 0)
        {
            return source.Keys.RandomItem();
        }

        int roll = Random.Shared.Next(totalWeight);

        uint weight = 0;
        foreach (T item in source.Keys)
        {
            weight += source[item];
            if (roll < weight)
            {
                return item;
            }
        }

        throw new InvalidOperationException("Weighted selection failed unexpectedly.");
    }

    public static IOrderedEnumerable<T> ThenByShuffled<T>(this IOrderedEnumerable<T> source)
    {
        return source.ThenBy(_ => Random.Shared.Next());
    }

    private static IOrderedEnumerable<T> OrderByShuffled<T>(this IEnumerable<T> source)
    {
        return source.OrderBy(_ => Random.Shared.Next());
    }
}