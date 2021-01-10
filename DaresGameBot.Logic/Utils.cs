using System;
using System.Collections.Generic;
using System.Linq;

namespace DaresGameBot.Logic
{
    internal static class Utils
    {
        public static readonly Random Random = new Random();

        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;

            while (n > 1)
            {
                int k = Random.Next(n);
                --n;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }

        public static Queue<T> ToShuffeledQueue<T>(this IEnumerable<T> items) => items.ToList().Shuffle().ToQueue();

        private static Queue<T> ToQueue<T>(this IEnumerable<T> items) => new Queue<T>(items);
    }
}
