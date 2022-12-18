using System;
using System.Collections.Generic;
using System.Linq;

namespace DaresGameBot;

internal sealed class Shuffler
{
    public List<T> Shuffle<T>(IEnumerable<T> items)
    {
        List<T> list = items.ToList();
        int n = list.Count;

        while (n > 1)
        {
            int k = _random.Next(n);
            --n;
            (list[k], list[n]) = (list[n], list[k]);
        }

        return list;
    }

    private readonly Random _random = new();
}