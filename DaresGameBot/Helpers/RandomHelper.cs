using System;
using System.Collections.Generic;
using System.Linq;
using GryphonUtilities.Extensions;

namespace DaresGameBot.Helpers;

internal static class RandomHelper
{
    public static T SelectItem<T>(Random random, IList<T> source) => source[random.Next(source.Count)];

    public static IEnumerable<T>? EnumerateUniqueItems<T>(Random random, IEnumerable<T> source, int count)
    {
        T[] items = source.ToArray();

        if (items.Length < count)
        {
            return null;
        }

        if (count == 1)
        {
            return SelectItem(random, items).Yield();
        }

        random.Shuffle(items);

        return count == items.Length ? items : items.Take(count);
    }

    public static T[] Shuffle<T>(Random random, IEnumerable<T> source)
    {
        T[] value = source.ToArray();
        random.Shuffle(value);
        return value;
    }
}