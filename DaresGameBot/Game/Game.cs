using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AbstractBot;
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

    public async Task StartNewGameAsync(ushort? playersAmount = null, float? choiceChance = null)
    {
        List<Deck<CardAction>> actionDecks;
        Deck<Card> questionsDeck;
        await using (await StatusMessage.CreateAsync(_bot, _chat, "Читаю колоды"))
        {
            actionDecks = await _bot.GameManager.GetActionDecksAsync();
            questionsDeck = await _bot.GameManager.GetQuestionsDeckAsync();
        }

        ushort players = playersAmount ?? _bot.Config.InitialPlayersAmount;
        float chance = choiceChance ?? _bot.Config.InitialChoiceChance;
        _game = new Data.Game(players, chance, actionDecks, questionsDeck);

        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine("🔥 Начинаем новую игру!");
        stringBuilder.AppendLine(_game.Players);
        stringBuilder.AppendLine(_game.Chance);
        await _bot.SendTextMessageAsync(_chat, stringBuilder.ToString());
    }

    public async Task<bool> UpdatePlayersAmountAsync(ushort playersAmount)
    {
        if (playersAmount <= 1)
        {
            return false;
        }

        if (_game is null)
        {
            await StartNewGameAsync(playersAmount);
        }
        else
        {
            _game.PlayersAmount = playersAmount;

            await _bot.SendTextMessageAsync(_chat, $"Принято! {_game.Players}");
        }
        return true;
    }

    public async Task<bool> UpdateChoiceChanceAsync(float choiceChance)
    {
        if (choiceChance is < 0.0f or > 1.0f)
        {
            return false;
        }

        if (_game is null)
        {
            await StartNewGameAsync(choiceChance: choiceChance);
        }
        else
        {
            _game.ChoiceChance = choiceChance;

            await _bot.SendTextMessageAsync(_chat, $"Принято! {_game.Chance}");
        }

        return true;
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