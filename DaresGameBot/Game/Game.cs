using System.Collections.Generic;
using System.Threading.Tasks;
using AbstractBot;
using AbstractBot.Configs.MessageTemplates;
using DaresGameBot.Game.Data;
using Telegram.Bot.Types;

namespace DaresGameBot.Game;

internal sealed class Game
{
    public bool IsActive() => _game is not null && _game.IsActive();

    public Game(Bot bot, Chat chat)
    {
        _bot = bot;
        _chat = chat;
    }

    public async Task StartNewGameAsync(byte? playersAmount = null, decimal? choiceChance = null)
    {
        List<Deck<CardAction>> actionDecks;
        Deck<Card> questionsDeck;
        await using (await StatusMessage.CreateAsync(_bot, _chat, _bot.Config.Texts.ReadingDecks))
        {
            actionDecks = await _bot.Repository.GetActionDecksAsync();
            questionsDeck = await _bot.Repository.GetQuestionsDeckAsync();
        }

        byte players = playersAmount ?? _bot.Config.InitialPlayersAmount;
        decimal chance = choiceChance ?? _bot.Config.InitialChoiceChance;
        _game = new Data.Game(players, chance, actionDecks, questionsDeck);

        MessageTemplateText playersText = _bot.Config.Texts.PlayersFormat.Format(players);
        MessageTemplateText startText = _bot.Config.Texts.NewGameFormat.Format(playersText, GetChanceText(chance));
        await startText.SendAsync(_bot, _chat);
    }

    public Task UpdatePlayersAmountAsync(byte playersAmount)
    {
        if (_game is null)
        {
            return StartNewGameAsync(playersAmount);
        }

        _game.PlayersAmount = playersAmount;

        MessageTemplateText playersText = _bot.Config.Texts.PlayersFormat.Format(_game.PlayersAmount);
        MessageTemplateText messageText = _bot.Config.Texts.AcceptedFormat.Format(playersText);
        return messageText.SendAsync(_bot, _chat);
    }

    public Task UpdateChoiceChanceAsync(decimal choiceChance)
    {
        if (_game is null)
        {
            return StartNewGameAsync(choiceChance: choiceChance);
        }

        _game.ChoiceChance = choiceChance;
        MessageTemplateText chanceText = GetChanceText(_game.ChoiceChance);
        MessageTemplateText messageText = _bot.Config.Texts.AcceptedFormat.Format(chanceText);
        return messageText.SendAsync(_bot, _chat);
    }

    public async Task DrawAsync(int replyToMessageId, bool action = true)
    {
        if (!IsActive())
        {
            await StartNewGameAsync();
            return;
        }

        Turn? turn;
        if (action)
        {
            turn = _game!.DrawAction();
            if (turn is null)
            {
                await StartNewGameAsync();
                return;
            }
        }
        else
        {
            turn = _game!.DrawQuestion();
        }

        MessageTemplateText message = turn.GetMessage(_game.PlayersAmount);
        message.ReplyToMessageId = replyToMessageId;
        await message.SendAsync(_bot, _chat);
        if (!IsActive())
        {
            _game = null;
            await _bot.Config.Texts.GameOver.SendAsync(_bot, _chat);
        }
    }

    private MessageTemplateText GetChanceText(decimal chance)
    {
        string formatted = chance.ToString(_bot.Config.Texts.PercentFormat);
        return _bot.Config.Texts.ChanceFormat.Format(_bot.Config.Texts.Choosable, formatted);
    }

    private Data.Game? _game;

    private readonly Bot _bot;
    private readonly Chat _chat;
}