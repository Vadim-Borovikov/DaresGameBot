using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AbstractBot;
using DaresGameBot.Game.Data;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DaresGameBot.Game
{
    internal sealed class Game
    {
        public const string DrawCaption = "Вытянуть фант";
        public const string NewGameCaption = "Новая игра";

        public Game(Bot.Bot bot, ChatId chatId)
        {
            _bot = bot;
            _chatId = chatId;
        }

        public async Task StartNewGameAsync(ushort? playersAmount = null, float? choiceChance = null)
        {
            Message statusMessage = await _bot.Client.SendTextMessageAsync(_chatId, "_Читаю колоды…_",
                ParseMode.Markdown, disableNotification: true);
            IEnumerable<Deck> decks = Manager.GetDecks(_bot);
            await _bot.Client.FinalizeStatusMessageAsync(statusMessage);

            _game = new Data.Game(playersAmount ?? _bot.Config.InitialPlayersAmount,
                choiceChance ?? _bot.Config.InitialChoiceChance, decks);

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("🔥 Начинаем новую игру!");
            stringBuilder.AppendLine(_game.Players);
            stringBuilder.AppendLine(_game.Chance);
            await _bot.Client.SendTextMessageAsync(_chatId, stringBuilder.ToString(), DrawCaption);
        }

        public async Task<bool> ChangePlayersAmountAsync(ushort playersAmount)
        {
            if (playersAmount <= 1)
            {
                return false;
            }

            if (_game == null)
            {
                await StartNewGameAsync(playersAmount);
            }
            else
            {
                _game.PlayersAmount = playersAmount;

                await _bot.Client.SendTextMessageAsync(_chatId, $"Принято! {_game.Players}", DrawCaption);
            }
            return true;
        }

        public async Task<bool> ChangeChoiceChanceAsync(float choiceChance)
        {
            if ((choiceChance < 0.0f) || (choiceChance > 1.0f))
            {
                return false;
            }

            if (_game == null)
            {
                await StartNewGameAsync(choiceChance: choiceChance);
            }
            else
            {
                _game.ChoiceChance = choiceChance;

                await _bot.Client.SendTextMessageAsync(_chatId, $"Принято! {_game.Chance}", DrawCaption);
            }

            return true;
        }

        public Task DrawAsync(int replyToMessageId)
        {
            if (_game == null)
            {
                return StartNewGameAsync();
            }

            Turn turn = _game.Draw();
            string text = turn.GetMessage(_game.PlayersAmount);

            string caption = DrawCaption;
            if (_game.Empty)
            {
                _game = null;
                caption = NewGameCaption;
            }
            return _bot.Client.SendTextMessageAsync(_chatId, text, caption, replyToMessageId);
        }

        private Data.Game _game;

        private readonly Bot.Bot _bot;
        private readonly ChatId _chatId;
    }
}