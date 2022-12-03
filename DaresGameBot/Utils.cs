using System;
using System.Collections.Generic;
using System.Linq;
using GryphonUtilities;
using Telegram.Bot.Types.ReplyMarkups;

namespace DaresGameBot;

internal static class Utils
{
    public static readonly ReplyKeyboardMarkup NewGameKeyboard = GetKeyboard(Game.Game.NewGameCaption.Yield());
    public static readonly ReplyKeyboardMarkup GameKeyboard = GetKeyboard(Game.Game.GameCaptions);

    public static readonly Random Random = new();

    public static ushort? ToUshort(object? o)
    {
        if (o is ushort u)
        {
            return u;
        }
        return ushort.TryParse(o?.ToString(), out u) ? u : null;
    }

    public static IList<T> Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;

        while (n > 1)
        {
            int k = Random.Next(n);
            --n;
            (list[k], list[n]) = (list[n], list[k]);
        }

        return list;
    }

    public static Queue<T> ToShuffeledQueue<T>(this IEnumerable<T> items) => items.ToList().Shuffle().ToQueue();

    private static Queue<T> ToQueue<T>(this IEnumerable<T> items) => new(items);

    private static ReplyKeyboardMarkup GetKeyboard(IEnumerable<string> buttonCaptions)
    {
        return new ReplyKeyboardMarkup(buttonCaptions.Select(c => new KeyboardButton(c)));
    }
}