using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DaresGameBot.Logic;
using DaresGameBot.Web.Models.Config;
using Newtonsoft.Json;
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

        public bool Valid => _game.Empty;

        public GameLogic(Settings settings, ITelegramBotClient client, ChatId chatId)
        {
            _decksJson = settings.DecksJson;
            _client = client;
            _chatId = chatId;

            _game = CreateNewGame(settings);
        }

        private Game CreateNewGame(Settings settings)
        {
            var decks = JsonConvert.DeserializeObject<List<Deck>>(_decksJson);
            return new Game(settings.InitialPlayersAmount, settings.InitialChoiceChance, decks);
        }

        private Game CreateNewGame()
        {
            var decks = JsonConvert.DeserializeObject<List<Deck>>(_decksJson);
            return new Game(_game.PlayersAmount, _game.ChoiceChance, decks);
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
                ? StartNewGameAsync()
                : _client.SendTextMessageAsync(_chatId, $"Принято! {_game.Players}", replyMarkup: GetKeyboard());
        }

        public Task ChangeChoiceChanceAsync(float choiceChance)
        {
            if ((choiceChance < 0.0f) || (choiceChance > 1.0f))
            {
                return Task.CompletedTask;
            }

            _game.ChoiceChance = choiceChance;

            return Valid
                ? StartNewGameAsync()
                : _client.SendTextMessageAsync(_chatId, $"Принято! {_game.Chance}", replyMarkup: GetKeyboard());
        }

        public Task DrawAsync()
        {
            if (Valid)
            {
                return StartNewGameAsync();
            }

            Turn turn = _game?.Draw();
            string text = turn?.GetMessage(_game.PlayersAmount) ?? "Игра закончена";
            return _client.SendTextMessageAsync(_chatId, text, replyMarkup: GetKeyboard());
        }

        private ReplyKeyboardMarkup GetKeyboard()
        {
            string caption = Valid ? NewGameCaption : DrawCaption;
            var button = new KeyboardButton(caption);
            var raw = new[] { button };
            return new ReplyKeyboardMarkup(raw, true);
        }

        private Game _game;

        private readonly string _decksJson;

        private readonly ITelegramBotClient _client;
        private readonly ChatId _chatId;
    }
}