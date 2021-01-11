using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DaresGameBot.Logic;
using GoogleSheetsManager;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Game = DaresGameBot.Logic.Game;

namespace DaresGameBot.Web.Models
{
    internal sealed class GameLogic
    {
        public const string DrawCaption = "Вытянуть фант";
        public const string NewGameCaption = "Новая игра";

        public bool Valid => !_game.Empty;

        public GameLogic(Config.Config config, Provider googleSheetsProvider, ITelegramBotClient client, ChatId chatId)
        {
            _googleRange = config.GoogleRange;
            _initialPlayersAmount = config.InitialPlayersAmount;
            _initialChoiceChance = config.InitialChoiceChance;
            _googleSheetsProvider = googleSheetsProvider;
            _client = client;
            _chatId = chatId;

            _game = CreateNewGame();
        }

        public Task StartNewGameAsync()
        {
            _game = CreateNewGame();

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("🔥 Начинаем новую игру!");
            stringBuilder.AppendLine(_game.Players);
            stringBuilder.AppendLine(_game.Chance);
            return _client.SendTextMessageAsync(_chatId, stringBuilder.ToString(), replyMarkup: GetKeyboard());
        }

        public Task ChangePlayersAmountAsync(ushort playersAmount)
        {
            if (playersAmount <= 0)
            {
                return Task.CompletedTask;
            }

            _game.PlayersAmount = playersAmount;

            return Valid
                ? _client.SendTextMessageAsync(_chatId, $"Принято! {_game.Players}", replyMarkup: GetKeyboard())
                : StartNewGameAsync();
        }

        public Task ChangeChoiceChanceAsync(float choiceChance)
        {
            if ((choiceChance < 0.0f) || (choiceChance > 1.0f))
            {
                return Task.CompletedTask;
            }

            _game.ChoiceChance = choiceChance;

            return Valid
                ? _client.SendTextMessageAsync(_chatId, $"Принято! {_game.Chance}", replyMarkup: GetKeyboard())
                : StartNewGameAsync();
        }

        public Task DrawAsync()
        {
            if (!Valid)
            {
                return StartNewGameAsync();
            }

            Turn turn = _game.Draw();
            string text = turn?.GetMessage(_game.PlayersAmount) ?? "Игра закончена";
            return _client.SendTextMessageAsync(_chatId, text, replyMarkup: GetKeyboard());
        }

        private Game CreateNewGame()
        {
            IEnumerable<Deck> decks = Utils.GetDecks(_googleSheetsProvider, _googleRange);
            return new Game(_initialPlayersAmount, _initialChoiceChance, decks);
        }

        private ReplyKeyboardMarkup GetKeyboard()
        {
            string caption = Valid ? DrawCaption : NewGameCaption;
            var button = new KeyboardButton(caption);
            var raw = new[] { button };
            return new ReplyKeyboardMarkup(raw, true);
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