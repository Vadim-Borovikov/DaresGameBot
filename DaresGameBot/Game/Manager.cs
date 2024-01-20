using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AbstractBot;
using AbstractBot.Configs.MessageTemplates;
using DaresGameBot.Game.Data;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DaresGameBot.Game;

internal sealed class Manager
{
    public Manager(Bot bot) => _bot = bot;

    public async Task StartAsync()
    {
        Chat chat = new()
        {
            Id = _bot.Config.LogsChatId,
            Type = ChatType.Private
        };

        _actions.Clear();
        _questions.Clear();

        await using (await StatusMessage.CreateAsync(_bot, chat, _bot.Config.Texts.ReadingDecks))
        {
            _actions.AddRange(await _bot.Actions.LoadAsync<CardAction>(_bot.Config.ActionsRange));
            _questions.AddRange(await _bot.Questions.LoadAsync<Card>(_bot.Config.QuestionsRange));
        }
    }

    public Data.Game StartNewGame(byte? playersAmount = null, decimal? choiceChance = null)
    {
        IList<Deck<CardAction>> actionDecks = GetActionDecks();
        Deck<Card> questionsDeck = CreateQuestionsDeck();

        byte players = playersAmount ?? _bot.Config.InitialPlayersAmount;
        decimal chance = choiceChance ?? _bot.Config.InitialChoiceChance;
        return new Data.Game(players, chance, actionDecks, questionsDeck);
    }

    public Task RepotNewGameAsync(Chat chat, Data.Game game)
    {
        MessageTemplateText playersText = _bot.Config.Texts.PlayersFormat.Format(game.PlayersAmount);
        MessageTemplateText startText =
            _bot.Config.Texts.NewGameFormat.Format(playersText, GetChanceText(game.ChoiceChance));
        return startText.SendAsync(_bot, chat);
    }

    private IList<Deck<CardAction>> GetActionDecks()
    {
        ReadOnlyCollection<CardAction> cards = _actions.AsReadOnly();
        return _actions.GroupBy(c => c.Tag).Select(g => CreateActionDeck(cards, g.Key)).ToList();
    }

    private static Deck<CardAction> CreateActionDeck(IReadOnlyList<CardAction> cards, string tag)
    {
        List<ushort> indexes = new();
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i].Tag.Equals(tag, StringComparison.OrdinalIgnoreCase))
            {
                indexes.Add((ushort) i);
            }
        }
        return new Deck<CardAction>(tag, cards, indexes);
    }

    private Deck<Card> CreateQuestionsDeck()
    {
        ReadOnlyCollection<Card> cards = _questions.AsReadOnly();
        List<ushort> indexes = Enumerable.Range(0, cards.Count).Select(i => (ushort)i).ToList();
        return new Deck<Card>(_bot.Config.Texts.QuestionsTag, cards, indexes);
    }

    public Task UpdatePlayersAmountAsync(Chat chat, Data.Game game, byte playersAmount)
    {
        game.PlayersAmount = playersAmount;

        MessageTemplateText playersText = _bot.Config.Texts.PlayersFormat.Format(game.PlayersAmount);
        MessageTemplateText messageText = _bot.Config.Texts.AcceptedFormat.Format(playersText);
        return messageText.SendAsync(_bot, chat);
    }

    public Task UpdateChoiceChanceAsync(Chat chat, Data.Game game, decimal choiceChance)
    {
        game.ChoiceChance = choiceChance;
        MessageTemplateText chanceText = GetChanceText(game.ChoiceChance);
        MessageTemplateText messageText = _bot.Config.Texts.AcceptedFormat.Format(chanceText);
        return messageText.SendAsync(_bot, chat);
    }

    public Turn? Draw(Data.Game game, bool action = true)
    {
        if (!game.IsActive())
        {
            return null;
        }

        if (action)
        {
            return game.DrawAction();
        }

        Turn? turn = game.DrawQuestion();
        if (turn is null)
        {
            game.SetQuestions(CreateQuestionsDeck());
            turn = game.DrawQuestion()!;
        }
        return turn;
    }

    public async Task RepotTurnAsync(Chat chat, Data.Game game, Turn turn, int replyToMessageId)
    {
        MessageTemplateText message = turn.GetMessage(game.PlayersAmount);
        message.ReplyToMessageId = replyToMessageId;
        await message.SendAsync(_bot, chat);
        if (!game.IsActive())
        {
            await _bot.Config.Texts.GameOver.SendAsync(_bot, chat);
        }
    }

    private MessageTemplateText GetChanceText(decimal chance)
    {
        string formatted = chance.ToString(_bot.Config.Texts.PercentFormat);
        return _bot.Config.Texts.ChanceFormat.Format(_bot.Config.Texts.Choosable, formatted);
    }

    private readonly List<CardAction> _actions = new();
    private readonly List<Card> _questions = new();

    private readonly Bot _bot;
}