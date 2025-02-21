using System.Collections.Generic;

namespace DaresGameBot.Helpers;

internal static class DictionaryHelper
{
    public static void CreateOrAdd<TKey>(this Dictionary<TKey, uint> dict, TKey key, uint value) where TKey : notnull
    {
        if (dict.ContainsKey(key))
        {
            dict[key] += value;
        }
        else
        {
            dict[key] = value;
        }
    }
}