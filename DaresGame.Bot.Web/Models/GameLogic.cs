using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DaresGame.Bot.Web.Models.Commands;
using DaresGame.Logic;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Game = DaresGame.Logic.Game;

namespace DaresGame.Bot.Web.Models
{
    public class GameLogic
    {
        private readonly TelegramBotClient _client;

        private Game _game;
        internal bool IsGameValid => (_game != null) && !_game.Empty;
        private readonly Settings _settings;

        private string Players => $"Игроков: {_settings.PlayersAmount}";
        private string Chance => $"Шанс на 🤩: {_settings.ChoiceChance:P0}";

        internal GameLogic(TelegramBotClient client, int initialPlayersAmount, float choiceChance, string decksJson)
        {
            var decks = JsonConvert.DeserializeObject<List<Deck>>(decksJson);

            _settings = new Settings
            {
                PlayersAmount = initialPlayersAmount,
                ChoiceChance = choiceChance,
                Decks = decks
            };

            _client = client;
        }

        internal Task StartNewGameAsync(Chat chat)
        {
            _game = new Game(_settings);

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("🔥 Начинаем новую игру!");
            stringBuilder.AppendLine(Players);
            stringBuilder.AppendLine(Chance);
            return _client.SendTextMessageAsync(chat, stringBuilder.ToString(), replyMarkup: GetKeyboard());
        }

        internal Task ChangePlayersAmountAsync(Chat chat, int playersAmount)
        {
            if (playersAmount <= 0)
            {
                return Task.CompletedTask;
            }

            _settings.PlayersAmount = playersAmount;

            if (!IsGameValid)
            {
                return StartNewGameAsync(chat);
            }

            _game.UpdateSettings(_settings);
            return _client.SendTextMessageAsync(chat, $"Принято! {Players}", replyMarkup: GetKeyboard());
        }

        internal Task ChangeChoiceChanceAsync(Chat chat, float choiceChance)
        {
            if ((choiceChance < 0.0f) || (choiceChance > 1.0f))
            {
                return Task.CompletedTask;
            }

            _settings.ChoiceChance = choiceChance;

            if (!IsGameValid)
            {
                return StartNewGameAsync(chat);
            }

            _game.UpdateSettings(_settings);
            return _client.SendTextMessageAsync(chat, $"Принято! {Chance}", replyMarkup: GetKeyboard());
        }

        internal Task DrawAsync(Chat chat)
        {
            if (!IsGameValid)
            {
                return StartNewGameAsync(chat);
            }

            Turn turn = _game?.Draw();
            string text = turn?.GetMessage(_settings.PlayersAmount) ?? "Игра закончена";
            return _client.SendTextMessageAsync(chat, text, replyMarkup: GetKeyboard());
        }

        private ReplyKeyboardMarkup GetKeyboard()
        {
            string caption = IsGameValid ? DrawCommand.Caption : NewCommand.Caption;
            var button = new KeyboardButton(caption);
            var raw = new[] { button };
            return new ReplyKeyboardMarkup(raw, true);
        }
    }
}