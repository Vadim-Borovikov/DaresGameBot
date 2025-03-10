using System.Collections.Generic;
using JetBrains.Annotations;

namespace DaresGameBot.Utilities.Extensions;

[PublicAPI]
public static class DictionaryExtensions
{
    public static void AddAll<TKey, TValue>(this Dictionary<TKey, TValue> tagret, Dictionary<TKey, TValue> source)
        where TKey : notnull
    {
        foreach (TKey key in source.Keys)
        {
            tagret[key] = source[key];
        }
    }
}