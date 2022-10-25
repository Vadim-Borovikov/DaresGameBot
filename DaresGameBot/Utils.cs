using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DaresGameBot;

internal static class Utils
{
    public static readonly Random Random = new();

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

    public static Task<Message> SendTextMessageAsync(this Bot bot, Chat chat, string text, string buttonCaption,
        int replyToMessageId = 0)
    {
        string[] buttonCaptions = { buttonCaption };
        return bot.SendTextMessageAsync(chat, text, buttonCaptions, replyToMessageId);
    }
    public static Task<Message> SendTextMessageAsync(this Bot bot, Chat chat, string text,
        IEnumerable<string> buttonCaptions, int replyToMessageId = 0)
    {
        ReplyKeyboardMarkup markup = new(buttonCaptions.Select(c => new KeyboardButton(c)));
        return bot.SendTextMessageAsync(chat, text, replyToMessageId: replyToMessageId, replyMarkup: markup);
    }

    public static ushort? ToUshort(object? o)
    {
        if (o is ushort u)
        {
            return u;
        }
        return ushort.TryParse(o?.ToString(), out u) ? u : null;
    }
}