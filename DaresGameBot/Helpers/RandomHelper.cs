using System;
using System.Collections.Generic;
using System.Linq;
using GryphonUtilities.Extensions;

namespace DaresGameBot.Helpers;

internal static class RandomHelper
{
    public static T SelectItem<T>(IList<T> source) => source[Random.Shared.Next(source.Count)];

    public static IEnumerable<T>? EnumerateUniqueItems<T>(IEnumerable<T> source, int count)
    {
        T[] items = source.ToArray();

        if (items.Length < count)
        {
            return null;
        }

        if (count == 1)
        {
            return SelectItem(items).Yield();
        }

        Random.Shared.Shuffle(items);

        return count == items.Length ? items : items.Take(count);
    }

    public static T[] Shuffle<T>(IEnumerable<T> source)
    {
        T[] value = source.ToArray();
        Random.Shared.Shuffle(value);
        return value;
    }
}