using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoogleSheetsManager;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DaresGameBot
{
    internal static class Utils
    {
        #region Google

        public static ushort? ToUshort(this IList<object> values, int index) => values.To(index, ToUshort);

        private static ushort? ToUshort(object o) => ushort.TryParse(o?.ToString(), out ushort i) ? (ushort?)i : null;

        #endregion // Google

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

        public static void AddRange<T>(this Queue<T> queue, IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                queue.Enqueue(item);
            }
        }

        public static Task<Message> SendTextMessageAsync(this ITelegramBotClient client, ChatId chatId, string text,
            string buttonCaption, string button2Caption = null, int replyToMessageId = 0)
        {
            var button = new KeyboardButton(buttonCaption);
            var raw = new List<KeyboardButton> { button };

            if (button2Caption != null)
            {
                var button2 = new KeyboardButton(button2Caption);
                raw.Add(button2);
            }

            var markup = new ReplyKeyboardMarkup(raw, true);
            return client.SendTextMessageAsync(chatId, text, replyToMessageId: replyToMessageId, replyMarkup: markup);
        }
    }
}
