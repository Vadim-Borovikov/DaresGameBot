using System.Collections.Concurrent;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DaresGameBot.Game
{
    internal static class Repository
    {
        public static Task StartNewGameAsync(Bot.Bot bot, ChatId chatId)
        {
            Manager manager = GetOrAddGameManager(bot, chatId);
            return manager.StartNewGameAsync();
        }

        public static Task<bool> ChangePlayersAmountAsync(ushort playersAmount, Bot.Bot bot, ChatId chatId)
        {
            Manager manager = GetOrAddGameManager(bot, chatId);
            return manager.ChangePlayersAmountAsync(playersAmount);
        }

        public static Task<bool> ChangeChoiceChanceAsync(float choiceChance, Bot.Bot bot, ChatId chatId)
        {
            Manager manager = GetOrAddGameManager(bot, chatId);
            return manager.ChangeChoiceChanceAsync(choiceChance);
        }

        public static Task DrawAsync(Bot.Bot bot, ChatId chatId, int replyToMessageId)
        {
            Manager manager = GetOrAddGameManager(bot, chatId);
            return manager.DrawAsync(replyToMessageId);
        }

        public static bool IsGameManagerValid(ChatId chatId)
        {
            return GameManagers.TryGetValue(chatId.Identifier, out Manager gameManager) && (gameManager != null);
        }

        private static Manager GetOrAddGameManager(Bot.Bot bot, ChatId chatId)
        {
            return GameManagers.GetOrAdd(chatId.Identifier, id => new Manager(bot, id));
        }

        private static readonly ConcurrentDictionary<long, Manager> GameManagers =
            new ConcurrentDictionary<long, Manager>();
    }
}