using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DaresGameBot.Game;
using GoogleSheetsManager;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
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

        public static Task<Message> FinalizeStatusMessageAsync(this ITelegramBotClient client, Message message,
            string postfix = "")
        {
            string text = $"_{message.Text}_ Готово.{postfix}";
            return client.EditMessageTextAsync(message.Chat, message.MessageId, text, ParseMode.Markdown);
        }

        public static Task<Message> SendStickerAsync(this ITelegramBotClient client, Message message,
            InputOnlineFile sticker)
        {
            return client.SendStickerAsync(message.Chat, sticker, replyToMessageId: message.MessageId);
        }

        public static Task<Message> SendTextMessageAsync(this ITelegramBotClient client, ChatId chatId, string text,
            int replyToMessageId, string buttonCaption)
        {
            var button = new KeyboardButton(buttonCaption);
            var raw = new[] { button };
            var markup = new ReplyKeyboardMarkup(raw, true);
            return client.SendTextMessageAsync(chatId, text, replyToMessageId: replyToMessageId, replyMarkup: markup);
        }

        public static async Task<string> GetNameAsync(this ITelegramBotClient client)
        {
            User me = await client.GetMeAsync();
            return me.Username;
        }

        public static IEnumerable<Deck> GetDecks(Provider googleSheetsProvider, string googleRange)
        {
            IList<Card> cards = DataManager.GetValues<Card>(googleSheetsProvider, googleRange);
            return cards.GroupBy(c => c.Tag)
                        .Select(g => CreateDeck(g.Key, g.ToList()));
        }

        private static Deck CreateDeck(string tag, IEnumerable<Card> cards)
        {
            return new Deck
            {
                Tag = tag,
                Cards = cards.ToList()
            };
        }
    }
}
