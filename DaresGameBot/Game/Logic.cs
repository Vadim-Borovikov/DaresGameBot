using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DaresGameBot.Bot;
using GoogleSheetsManager;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DaresGameBot.Game
{
    internal sealed class Logic
    {
        public const string DrawCaption = "Вытянуть фант";
        public const string NewGameCaption = "Новая игра";

        public Logic(Config config, Provider googleSheetsProvider, ITelegramBotClient client, ChatId chatId)
        {
            _googleRange = config.GoogleRange;
            _initialPlayersAmount = config.InitialPlayersAmount;
            _initialChoiceChance = config.InitialChoiceChance;
            _googleSheetsProvider = googleSheetsProvider;
            _client = client;
            _chatId = chatId;
        }

        public async Task StartNewGameAsync(int replyToMessageId, ushort? playersAmount = null,
            float? choiceChance = null)
        {
            Message statusMessage = await _client.SendTextMessageAsync(_chatId, "_Читаю колоды…_", ParseMode.Markdown,
                disableNotification: true);
            IEnumerable<Deck> decks = Utils.GetDecks(_googleSheetsProvider, _googleRange);
            await _client.FinalizeStatusMessageAsync(statusMessage);

            _game = new Game(playersAmount ?? _initialPlayersAmount, choiceChance ?? _initialChoiceChance, decks);

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("🔥 Начинаем новую игру!");
            stringBuilder.AppendLine(_game.Players);
            stringBuilder.AppendLine(_game.Chance);
            await _client.SendTextMessageAsync(_chatId, stringBuilder.ToString(), replyToMessageId, DrawCaption);
        }

        public async Task<bool> ChangePlayersAmountAsync(ushort playersAmount, int replyToMessageId)
        {
            if (playersAmount <= 1)
            {
                return false;
            }

            if (_game == null)
            {
                await StartNewGameAsync(replyToMessageId, playersAmount);
            }
            else
            {
                _game.PlayersAmount = playersAmount;

                await
                    _client.SendTextMessageAsync(_chatId, $"Принято! {_game.Players}", replyToMessageId, DrawCaption);
            }
            return true;
        }

        public async Task<bool> ChangeChoiceChanceAsync(float choiceChance, int replyToMessageId)
        {
            if ((choiceChance < 0.0f) || (choiceChance > 1.0f))
            {
                return false;
            }

            if (_game == null)
            {
                await StartNewGameAsync(replyToMessageId, choiceChance: choiceChance);
            }
            else
            {
                _game.ChoiceChance = choiceChance;

                await _client.SendTextMessageAsync(_chatId, $"Принято! {_game.Chance}", replyToMessageId, DrawCaption);
            }

            return true;
        }

        public Task DrawAsync(int replyToMessageId)
        {
            if (_game == null)
            {
                return StartNewGameAsync(replyToMessageId);
            }

            Turn turn = _game.Draw();
            string text = turn.GetMessage(_game.PlayersAmount);

            string caption = DrawCaption;
            if (_game.Empty)
            {
                _game = null;
                caption = NewGameCaption;
            }
            return _client.SendTextMessageAsync(_chatId, text, replyToMessageId, caption);
        }

        private Game _game;

        private readonly Provider _googleSheetsProvider;
        private readonly string _googleRange;
        private readonly ushort _initialPlayersAmount;
        private readonly float _initialChoiceChance;
        private readonly ITelegramBotClient _client;
        private readonly ChatId _chatId;
    }
}