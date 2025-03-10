using System;
using System.Collections.Generic;
using System.Linq;

namespace DaresGameBot.Utilities;

internal static class RandomHelper
{
    public static T SelectItem<T>(IList<T> source) => source[Random.Shared.Next(source.Count)];

    public static T[] Shuffle<T>(IEnumerable<T> source)
    {
        T[] value = source.ToArray();
        Random.Shared.Shuffle(value);
        return value;
    }
}