using System.Collections.Concurrent;
using System.Threading.Tasks;
using DaresGameBot.Bot;
using GoogleSheetsManager;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DaresGameBot.Game
{
    internal static class Repository
    {
        public static Task StartNewGameAsync(BotConfig config, Provider googleSheetsProvider,
            ITelegramBotClient client, ChatId chatId, int replyToMessageId)
        {
            Logic game = GetOrAddGame(config, googleSheetsProvider, client, chatId);
            return game.StartNewGameAsync(replyToMessageId);
        }

        public static Task<bool> ChangePlayersAmountAsync(ushort playersAmount, BotConfig config,
            Provider googleSheetsProvider, ITelegramBotClient client, ChatId chatId, int replyToMessageId)
        {
            Logic game = GetOrAddGame(config, googleSheetsProvider, client, chatId);
            return game.ChangePlayersAmountAsync(playersAmount, replyToMessageId);
        }

        public static Task<bool> ChangeChoiceChanceAsync(float choiceChance, BotConfig config,
            Provider googleSheetsProvider, ITelegramBotClient client, ChatId chatId, int replyToMessageId)
        {
            Logic game = GetOrAddGame(config, googleSheetsProvider, client, chatId);
            return game.ChangeChoiceChanceAsync(choiceChance, replyToMessageId);
        }

        public static Task DrawAsync(BotConfig config, Provider googleSheetsProvider, ITelegramBotClient client,
            ChatId chatId, int replyToMessageId)
        {
            Logic game = GetOrAddGame(config, googleSheetsProvider, client, chatId);
            return game.DrawAsync(replyToMessageId);
        }

        public static bool IsGameValid(ChatId chatId)
        {
            return Games.TryGetValue(chatId.Identifier, out Logic game) && (game != null);
        }

        private static Logic GetOrAddGame(BotConfig config, Provider googleSheetsProvider,
            ITelegramBotClient client, ChatId chatId)
        {
            return Games.GetOrAdd(chatId.Identifier, id => new Logic(config, googleSheetsProvider, client, id));
        }

        private static readonly ConcurrentDictionary<long, Logic> Games =
            new ConcurrentDictionary<long, Logic>();
    }
}