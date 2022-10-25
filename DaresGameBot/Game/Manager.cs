﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DaresGameBot.Game.Data;
using GoogleSheetsManager;
using GryphonUtilities;
using Telegram.Bot.Types;

namespace DaresGameBot.Game;

internal static class Manager
{
    public static Task StartNewGameAsync(Bot bot, Chat chat)
    {
        Game manager = GetOrAddGameManager(bot, chat);
        return manager.StartNewGameAsync();
    }

    public static Task<bool> ChangePlayersAmountAsync(ushort playersAmount, Bot bot, Chat chat)
    {
        Game manager = GetOrAddGameManager(bot, chat);
        return manager.ChangePlayersAmountAsync(playersAmount);
    }

    public static Task<bool> ChangeChoiceChanceAsync(float choiceChance, Bot bot, Chat chat)
    {
        Game manager = GetOrAddGameManager(bot, chat);
        return manager.ChangeChoiceChanceAsync(choiceChance);
    }

    public static Task DrawAsync(Bot bot, Chat chat, int replyToMessageId, bool action = true)
    {
        Game manager = GetOrAddGameManager(bot, chat);
        return manager.DrawAsync(replyToMessageId, action);
    }

    public static bool IsGameManagerValid(Chat chat) => GameManagers.ContainsKey(chat.Id);

    private static Game GetOrAddGameManager(Bot bot, Chat chat)
    {
        return GameManagers.GetOrAdd(chat.Id, _ => new Game(bot, chat));
    }

    public static async Task<List<Deck<CardAction>>> GetActionDecksAsync(Bot bot)
    {
        string range = bot.Config.ActionsGoogleRange.GetValue(nameof(bot.Config.ActionsGoogleRange));
        SheetData<CardAction> cards = await DataManager.GetValuesAsync<CardAction>(bot.GoogleSheetsProvider, range);
        return cards.Instances.GroupBy(c => c.Tag).Select(g => CreateActionDeck(g.Key, g.ToList())).ToList();
    }

    public static async Task<Deck<Card>> GetQuestionsDeckAsync(Bot bot)
    {
        string range = bot.Config.QuestionsGoogleRange.GetValue(nameof(bot.Config.QuestionsGoogleRange));
        SheetData<Card> cards = await DataManager.GetValuesAsync<Card>(bot.GoogleSheetsProvider, range);
        return new Deck<Card>("❓") { Cards = cards.Instances.ToList() };
    }

    private static Deck<CardAction> CreateActionDeck(string tag, IEnumerable<CardAction> cards)
    {
        return new Deck<CardAction>(tag) { Cards = cards.ToList() };
    }

    private static readonly ConcurrentDictionary<long, Game> GameManagers = new();
}