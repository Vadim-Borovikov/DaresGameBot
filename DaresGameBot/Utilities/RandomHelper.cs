using System;
using System.Collections.Generic;
using System.Linq;

namespace DaresGameBot.Utilities;

internal static class RandomHelper
{
    public static T RandomItem<T>(this IEnumerable<T> source) => source.OrderByShuffled().First();

    public static IOrderedEnumerable<T> ThenByShuffled<T>(this IOrderedEnumerable<T> source)
    {
        return source.ThenBy(_ => Random.Shared.Next());
    }

    private static IOrderedEnumerable<T> OrderByShuffled<T>(this IEnumerable<T> source)
    {
        return source.OrderBy(_ => Random.Shared.Next());
    }
}