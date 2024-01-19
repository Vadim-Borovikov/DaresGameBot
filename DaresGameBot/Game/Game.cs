using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AbstractBot;
using AbstractBot.Configs.MessageTemplates;
using DaresGameBot.Game.Data;
using Telegram.Bot.Types;

namespace DaresGameBot.Game;

internal sealed class Game
{
    public const string DrawActionCaption = "Вытянуть действие";
    public const string DrawQuestionCaption = "Вытянуть вопрос";
    public const string NewGameCaption = "Новая игра";
    public static readonly IEnumerable<string> GameCaptions = new[]
    {
        DrawActionCaption,
        DrawQuestionCaption
    };

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
        await using (await StatusMessage.CreateAsync(_bot, _chat, new MessageTemplateText("Читаю колоды")))
        {
            actionDecks = await _bot.GameManager.GetActionDecksAsync();
            questionsDeck = await _bot.GameManager.GetQuestionsDeckAsync();
        }

        byte players = playersAmount ?? _bot.Config.InitialPlayersAmount;
        decimal chance = choiceChance ?? _bot.Config.InitialChoiceChance;
        _game = new Data.Game(players, chance, actionDecks, questionsDeck);

        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine("🔥 Начинаем новую игру!");
        stringBuilder.AppendLine(_game.Players);
        stringBuilder.AppendLine(_game.Chance);
        await _bot.SendTextMessageAsync(_chat, stringBuilder.ToString());
    }

    public Task UpdatePlayersAmountAsync(byte playersAmount)
    {
        if (_game is null)
        {
            return StartNewGameAsync(playersAmount);
        }

        _game.PlayersAmount = playersAmount;

        return _bot.SendTextMessageAsync(_chat, $"Принято! {_game.Players}");
    }

    public Task UpdateChoiceChanceAsync(decimal choiceChance)
    {
        if (_game is null)
        {
            return StartNewGameAsync(choiceChance: choiceChance);
        }

        _game.ChoiceChance = choiceChance;
        return _bot.SendTextMessageAsync(_chat, $"Принято! {_game.Chance}");
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
            // ReSharper disable NullableWarningSuppressionIsUsed
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
            // ReSharper restore NullableWarningSuppressionIsUsed
        }

        string text = turn.GetMessage(_game.PlayersAmount);

        await _bot.SendTextMessageAsync(_chat, text, replyToMessageId: replyToMessageId);
        if (!IsActive())
        {
            _game = null;
            await _bot.SendTextMessageAsync(_chat, "Игра закончена!");
        }
    }

    private Data.Game? _game;

    private readonly Bot _bot;
    private readonly Chat _chat;
}