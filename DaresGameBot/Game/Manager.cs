﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DaresGameBot.Game.Data;
using Telegram.Bot.Types;

namespace DaresGameBot.Game;

internal sealed class Manager
{
    public Manager(Bot bot) => _bot = bot;

    public bool CheckGame(Chat chat)
    {
        Game manager = GetOrAddGameManager(chat);
        return manager.IsActive();
    }

    public Task StartNewGameAsync(Chat chat)
    {
        Game manager = GetOrAddGameManager(chat);
        return manager.StartNewGameAsync();
    }

    public Task UpdatePlayersAmountAsync(byte playersAmount, Chat chat)
    {
        Game manager = GetOrAddGameManager(chat);
        return manager.UpdatePlayersAmountAsync(playersAmount);
    }

    public Task UpdateChoiceChanceAsync(decimal choiceChance, Chat chat)
    {
        Game manager = GetOrAddGameManager(chat);
        return manager.UpdateChoiceChanceAsync(choiceChance);
    }

    public Task DrawAsync(Chat chat, int replyToMessageId, bool action = true)
    {
        Game manager = GetOrAddGameManager(chat);
        return manager.DrawAsync(replyToMessageId, action);
    }

    private Game GetOrAddGameManager(Chat chat) => _gameManagers.GetOrAdd(chat.Id, _ => new Game(_bot, chat));

    public async Task<List<Deck<CardAction>>> GetActionDecksAsync()
    {
        List<CardAction> cards = await _bot.Actions.LoadAsync<CardAction>(_bot.Config.ActionsRange);
        return cards.GroupBy(c => c.Tag).Select(g => CreateActionDeck(g.Key, g.ToList())).ToList();
    }

    public async Task<Deck<Card>> GetQuestionsDeckAsync()
    {
        List<Card> cards = await _bot.Questions.LoadAsync<Card>(_bot.Config.QuestionsRange);
        return new Deck<Card>(_bot.Config.Texts.QuestionsTag) { Cards = cards };
    }

    private static Deck<CardAction> CreateActionDeck(string tag, IEnumerable<CardAction> cards)
    {
        return new Deck<CardAction>(tag) { Cards = cards.ToList() };
    }

    private readonly ConcurrentDictionary<long, Game> _gameManagers = new();
    private readonly Bot _bot;
}