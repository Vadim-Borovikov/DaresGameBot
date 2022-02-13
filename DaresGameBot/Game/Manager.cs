using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DaresGameBot.Game.Data;
using GoogleSheetsManager;

namespace DaresGameBot.Game;

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

    public static bool IsGameManagerValid(long id) => GameManagers.ContainsKey(id);

    private static Game GetOrAddGameManager(Bot.Bot bot, long id) => GameManagers.GetOrAdd(id, i => new Game(bot, i));

    public static async Task<List<Deck<CardAction>>> GetActionDecksAsync(Bot.Bot bot)
    {
        string range = bot.Config.ActionsGoogleRange
                       ?? throw new NullReferenceException(nameof(bot.Config.ActionsGoogleRange));
        IList<CardAction> cards = await DataManager.GetValuesAsync<CardAction>(bot.GoogleSheetsProvider, range);
        return cards.GroupBy(c => c.Tag)
                    .Select(g => CreateActionDeck(g.Key, g.ToList()))
                    .ToList();
    }

    public static async Task<Deck<Card>> GetQuestionsDeckAsync(Bot.Bot bot)
    {
        string range = bot.Config.QuestionsGoogleRange
                       ?? throw new NullReferenceException(nameof(bot.Config.QuestionsGoogleRange));
        IList<Card> cards = await DataManager.GetValuesAsync<Card>(bot.GoogleSheetsProvider, range);
        return new Deck<Card>("❓") { Cards = cards.ToList() };
    }

    private static Deck<CardAction> CreateActionDeck(string tag, IEnumerable<CardAction> cards)
    {
        return new Deck<CardAction>(tag) { Cards = cards.ToList() };
    }

    private static readonly ConcurrentDictionary<long, Game> GameManagers = new();
}
