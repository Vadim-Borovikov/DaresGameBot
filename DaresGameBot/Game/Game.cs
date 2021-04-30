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
        public const string RejectCaptionPrefix = "Отказ";

        private const string RejectCaptionFormat = RejectCaptionPrefix + " ({0})";

        public Game(Bot.Bot bot, ChatId chatId)
        {
            _bot = bot;
            _chatId = chatId;
        }

        public async Task StartNewGameAsync(ushort? playersAmount = null, float? choiceChance = null,
            ushort? rejectsAmount = null)
        {
            Message statusMessage = await _bot.Client.SendTextMessageAsync(_chatId, "_Читаю колоды…_",
                ParseMode.Markdown, disableNotification: true);
            IEnumerable<Deck> decks = Manager.GetDecks(_bot);
            await _bot.Client.FinalizeStatusMessageAsync(statusMessage);

            _game = new Data.Game(playersAmount ?? _bot.Config.InitialPlayersAmount,
                choiceChance ?? _bot.Config.InitialChoiceChance, rejectsAmount ?? _bot.Config.InitialRejectsAmount,
                decks);

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Начинаем новую игру 🎉");
            stringBuilder.AppendLine(_game.Players);
            stringBuilder.AppendLine(_game.Chance);
            stringBuilder.AppendLine(_game.Rejects);
            await SendMessageAsync(stringBuilder.ToString());
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

                await SendMessageAsync($"Принято! {_game.Players}");
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

                await SendMessageAsync($"Принято! {_game.Chance}");
            }

            return true;
        }

        public async Task<bool> ChangeRejectsAmountAsync(ushort rejectsAmount)
        {
            if (_game == null)
            {
                await StartNewGameAsync(rejectsAmount: rejectsAmount);
            }
            else
            {
                _game.RejectsAmount = rejectsAmount;

                await SendMessageAsync($"Принято! {_game.Rejects}");
            }
            return true;
        }

        public Task DrawAsync(int replyToMessageId)
        {
            if (_game == null)
            {
                return StartNewGameAsync();
            }

            Card card = _game.Draw();
            _currentTurn = _game.CreateTurn(card);

            return SendMessageAsync(_currentTurn.GetMessage(), replyToMessageId);
        }

        private Turn _currentTurn;

        public async Task<bool> RerollPartnersAsync(int replyToMessageId)
        {
            if (_game == null)
            {
                await StartNewGameAsync();
                return false;
            }

            if (_currentTurn.Rejects <= 0)
            {
                return false;
            }

            if (_currentTurn.Card.PartnersToAssign >= _game.PlayersAmount)
            {
                return false;
            }

            _game.Reroll(_currentTurn);

            await SendMessageAsync(_currentTurn.GetMessage(), replyToMessageId);
            return true;
        }

        private Task SendMessageAsync(string text, int replyToMessageId = 0)
        {
            string caption = DrawCaption;
            string caption2 = null;
            if (_game.Empty)
            {
                _game = null;
                caption = NewGameCaption;
            }
            else if ((_currentTurn?.Card?.PartnersToAssign > 0) && (_currentTurn?.Rejects > 0))
            {
                caption2 = string.Format(RejectCaptionFormat, _currentTurn.Rejects);
            }
            return _bot.Client.SendTextMessageAsync(_chatId, text, caption, caption2, replyToMessageId);
        }

        private Data.Game _game;

        private readonly Bot.Bot _bot;
        private readonly ChatId _chatId;
    }
}