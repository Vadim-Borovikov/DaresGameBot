using System;
using System.Collections.Generic;
using System.Linq;

namespace DaresGame.Logic
{
    internal static class Utils
    {
        internal static readonly Random Random = new Random();

        private static IList<T> Shuffle<T>(this IList<T> list)
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

        private static Queue<T> ToQueue<T>(this IEnumerable<T> items) => new Queue<T>(items);

        public static Queue<T> ToShuffeledQueue<T>(this IEnumerable<T> items) => items.ToList().Shuffle().ToQueue();
    }
}
