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
        public static Task StartNewGameAsync(Bot.Bot bot, long id)
        {
            Game manager = GetOrAddGameManager(bot, id);
            return manager.StartNewGameAsync();
        }

        public static Task<bool> ChangePlayersAmountAsync(ushort playersAmount, Bot.Bot bot, long id)
        {
            Game manager = GetOrAddGameManager(bot, id);
            return manager.ChangePlayersAmountAsync(playersAmount);
        }

        public static Task<bool> ChangeChoiceChanceAsync(float choiceChance, Bot.Bot bot, long id)
        {
            Game manager = GetOrAddGameManager(bot, id);
            return manager.ChangeChoiceChanceAsync(choiceChance);
        }

        public static Task DrawAsync(Bot.Bot bot, long id, int replyToMessageId, bool action = true)
        {
            Game manager = GetOrAddGameManager(bot, id);
            return manager.DrawAsync(replyToMessageId, action);
        }

        public static bool IsGameManagerValid(long id)
        {
            return GameManagers.TryGetValue(id, out Game gameManager) && (gameManager != null);
        }

        private static Game GetOrAddGameManager(Bot.Bot bot, long id)
        {
            return GameManagers.GetOrAdd(id, i => new Game(bot, i));
        }

        public static async Task<List<Deck<CardAction>>> GetActionDecksAsync(Bot.Bot bot)
        {
            IList<CardAction> cards =
                await DataManager.GetValuesAsync<CardAction>(bot.GoogleSheetsProvider, bot.Config.ActionsGoogleRange);
            return cards.GroupBy(c => c.Tag)
                        .Select(g => CreateActionDeck(g.Key, g.ToList()))
                        .ToList();
        }

        public static async Task<Deck<Card>> GetQuestionsDeckAsync(Bot.Bot bot)
        {
            IList<Card> cards =
                await DataManager.GetValuesAsync<Card>(bot.GoogleSheetsProvider, bot.Config.QuestionsGoogleRange);
            return new Deck<Card>
            {
                Tag = "❓",
                Cards = cards.ToList()
            };
        }

        private static Deck<CardAction> CreateActionDeck(string tag, IEnumerable<CardAction> cards)
        {
            return new Deck<CardAction>
            {
                Tag = tag,
                Cards = cards.ToList()
            };
        }

        private static readonly ConcurrentDictionary<long, Game> GameManagers =
            new ConcurrentDictionary<long, Game>();
    }
}