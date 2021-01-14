using System.Collections.Concurrent;
using System.Threading.Tasks;
using GoogleSheetsManager;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DaresGameBot.Web.Models
{
    internal static class GamesRepository
    {
        public static Task StartNewGameAsync(Config.Config config, Provider googleSheetsProvider,
            ITelegramBotClient client, ChatId chatId, int replyToMessageId)
        {
            GameLogic game = GetOrAddGame(config, googleSheetsProvider, client, chatId);
            return game.StartNewGameAsync(replyToMessageId);
        }

        public static Task<bool> ChangePlayersAmountAsync(ushort playersAmount, Config.Config config,
            Provider googleSheetsProvider, ITelegramBotClient client, ChatId chatId, int replyToMessageId)
        {
            GameLogic game = GetOrAddGame(config, googleSheetsProvider, client, chatId);
            return game.ChangePlayersAmountAsync(playersAmount, replyToMessageId);
        }

        public static Task<bool> ChangeChoiceChanceAsync(float choiceChance, Config.Config config,
            Provider googleSheetsProvider, ITelegramBotClient client, ChatId chatId, int replyToMessageId)
        {
            GameLogic game = GetOrAddGame(config, googleSheetsProvider, client, chatId);
            return game.ChangeChoiceChanceAsync(choiceChance, replyToMessageId);
        }

        public static Task DrawAsync(Config.Config config, Provider googleSheetsProvider, ITelegramBotClient client,
            ChatId chatId, int replyToMessageId)
        {
            GameLogic game = GetOrAddGame(config, googleSheetsProvider, client, chatId);
            return game.DrawAsync(replyToMessageId);
        }

        public static bool IsGameValid(ChatId chatId)
        {
            return Games.TryGetValue(chatId.Identifier, out GameLogic game) && (game != null);
        }

        private static GameLogic GetOrAddGame(Config.Config config, Provider googleSheetsProvider,
            ITelegramBotClient client, ChatId chatId)
        {
            return Games.GetOrAdd(chatId.Identifier, id => new GameLogic(config, googleSheetsProvider, client, id));
        }

        private static readonly ConcurrentDictionary<long, GameLogic> Games =
            new ConcurrentDictionary<long, GameLogic>();
    }
}