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

        public static Task DrawAsync(Bot.Bot bot, ChatId chatId, int replyToMessageId, bool action = true)
        {
            Game manager = GetOrAddGameManager(bot, chatId);
            return manager.DrawAsync(replyToMessageId, action);
        }

        public static bool IsGameManagerValid(ChatId chatId)
        {
            return GameManagers.TryGetValue(chatId.Identifier, out Game gameManager) && (gameManager != null);
        }

        private static Game GetOrAddGameManager(Bot.Bot bot, ChatId chatId)
        {
            return GameManagers.GetOrAdd(chatId.Identifier, id => new Game(bot, id));
        }

        public static IEnumerable<Deck<CardAction>> GetActionDecks(Bot.Bot bot)
        {
            IList<CardAction> cards =
                DataManager.GetValues<CardAction>(bot.GoogleSheetsProvider, bot.Config.ActionsGoogleRange);
            return cards.GroupBy(c => c.Tag)
                        .Select(g => CreateActionDeck(g.Key, g.ToList()));
        }

        public static Deck<Card> GetQuestionsDeck(Bot.Bot bot)
        {
            IList<Card> cards = DataManager.GetValues<Card>(bot.GoogleSheetsProvider, bot.Config.QuestionsGoogleRange);
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