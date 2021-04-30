using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DaresGameBot.Game.Data;
using GoogleSheetsManager;
using Telegram.Bot.Types;

namespace DaresGameBot.Game
{
    internal static class Manager
    {
        public static Task StartNewGameAsync(Bot.Bot bot, ChatId chatId)
        {
            Game manager = GetOrAddGameManager(bot, chatId);
            return manager.StartNewGameAsync();
        }

        public static Task<bool> ChangePlayersAmountAsync(ushort playersAmount, Bot.Bot bot, ChatId chatId)
        {
            Game manager = GetOrAddGameManager(bot, chatId);
            return manager.ChangePlayersAmountAsync(playersAmount);
        }

        public static Task<bool> ChangeChoiceChanceAsync(float choiceChance, Bot.Bot bot, ChatId chatId)
        {
            Game manager = GetOrAddGameManager(bot, chatId);
            return manager.ChangeChoiceChanceAsync(choiceChance);
        }

        public static Task<bool> ChangeRejectsAmountAsync(ushort rejectsAmount, Bot.Bot bot, ChatId chatId)
        {
            Game manager = GetOrAddGameManager(bot, chatId);
            return manager.ChangeRejectsAmountAsync(rejectsAmount);
        }

        public static Task<bool> RerollPartnersAsync(Bot.Bot bot, ChatId chatId, int replyToMessageId)
        {
            Game manager = GetOrAddGameManager(bot, chatId);
            return manager.RerollPartnersAsync(replyToMessageId);
        }

        public static Task DrawAsync(Bot.Bot bot, ChatId chatId, int replyToMessageId)
        {
            Game manager = GetOrAddGameManager(bot, chatId);
            return manager.DrawAsync(replyToMessageId);
        }

        public static bool IsGameManagerValid(ChatId chatId)
        {
            return GameManagers.TryGetValue(chatId.Identifier, out Game gameManager) && (gameManager != null);
        }

        private static Game GetOrAddGameManager(Bot.Bot bot, ChatId chatId)
        {
            return GameManagers.GetOrAdd(chatId.Identifier, id => new Game(bot, id));
        }


        public static IEnumerable<Deck> GetDecks(Bot.Bot bot)
        {
            IList<Card> cards = DataManager.GetValues<Card>(bot.GoogleSheetsProvider, bot.Config.GoogleRange);
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

        private static readonly ConcurrentDictionary<long, Game> GameManagers =
            new ConcurrentDictionary<long, Game>();
    }
}