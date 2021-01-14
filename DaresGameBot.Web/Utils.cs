using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DaresGameBot.Logic;
using DaresGameBot.Web.Models;
using GoogleSheetsManager;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using File = System.IO.File;

namespace DaresGameBot.Web
{
    internal static class Utils
    {
        #region Google

        public static ushort? ToUshort(this IList<object> values, int index) => values.Extract(index, ToUshort);

        private static ushort? ToUshort(object o) => ushort.TryParse(o?.ToString(), out ushort i) ? (ushort?)i : null;

        #endregion // Google

        public static Task<Message> SendStickerAsync(this ITelegramBotClient client, Message message,
            InputOnlineFile sticker)
        {
            return client.SendStickerAsync(message.Chat, sticker, replyToMessageId: message.MessageId);
        }

        public static async Task<string> GetNameAsync(this ITelegramBotClient client)
        {
            User me = await client.GetMeAsync();
            return me.Username;
        }

        public static IEnumerable<Deck> GetDecks(Provider googleSheetsProvider, string googleRange)
        {
            IList<LoadableCard> cards = DataManager.GetValues<LoadableCard>(googleSheetsProvider, googleRange);
            return cards.GroupBy(c => c.Tag)
                        .Select(g => CreateDeck(g.Key, g.ToList()));
        }

        private static Deck CreateDeck(string tag, IEnumerable<LoadableCard> cards)
        {
            return new Deck
            {
                Tag = tag,
                Cards = cards.Cast<Card>().ToList()
            };
        }

        public static void LogException(Exception ex)
        {
            File.AppendAllText(ExceptionsLogPath, $"{ex}{Environment.NewLine}");
        }

        private const string ExceptionsLogPath = "errors.txt";
    }
}
