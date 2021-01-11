using System.Collections.Concurrent;
using System.Threading.Tasks;
using DaresGameBot.Web.Models.Config;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DaresGameBot.Web.Models
{
    internal static class GamesRepository
    {
        public static Task StartNewGameAsync(Settings settings, ITelegramBotClient client, ChatId chatId)
        {
            GameLogic game = GetOrAddGame(settings, client, chatId);
            return game.StartNewGameAsync();
        }

        public static Task ChangePlayersAmountAsync(ushort playersAmount, Settings settings, ITelegramBotClient client,
            ChatId chatId)
        {
            GameLogic game = GetOrAddGame(settings, client, chatId);
            return game.ChangePlayersAmountAsync(playersAmount);
        }

        public static Task ChangeChoiceChanceAsync(float choiceChance, Settings settings, ITelegramBotClient client,
            ChatId chatId)
        {
            GameLogic game = GetOrAddGame(settings, client, chatId);
            return game.ChangeChoiceChanceAsync(choiceChance);
        }

        public static Task DrawAsync(Settings settings, ITelegramBotClient client, ChatId chatId)
        {
            GameLogic game = GetOrAddGame(settings, client, chatId);
            return game.DrawAsync();
        }

        public static bool IsGameValid(ChatId chatId)
        {
            return Games.TryGetValue(chatId.Identifier, out GameLogic game) && game.Valid;
        }

        private static GameLogic GetOrAddGame(Settings settings, ITelegramBotClient client, ChatId chatId)
        {
            return Games.GetOrAdd(chatId.Identifier, id => new GameLogic(settings, client, id));
        }

        private static readonly ConcurrentDictionary<long, GameLogic> Games =
            new ConcurrentDictionary<long, GameLogic>();
    }
}