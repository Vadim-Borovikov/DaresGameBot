using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DaresGame.Bot.Web.Models.Commands;
using DaresGame.Logic;
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

        private string Players => $"Игроков: {_settings.PlayersNumber}";
        private string Chance => $"Шанс на 🤩: {_settings.ChoiceChance:P0}";

        internal GameLogic(TelegramBotClient client, int playersNumber, double choiceChance, IEnumerable<Deck> decks)
        {
            _settings = new Settings
            {
                PlayersNumber = playersNumber,
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

        internal Task ChangePlayersNumberAsync(Chat chat, int playersNumber)
        {
            if (playersNumber <= 0)
            {
                return Task.CompletedTask;
            }

            _settings.PlayersNumber = playersNumber;

            if (!IsGameValid)
            {
                return StartNewGameAsync(chat);
            }

            _game.UpdateSettings(_settings);
            return _client.SendTextMessageAsync(chat, $"Принято! {Players}", replyMarkup: GetKeyboard());
        }

        internal Task ChangeChoiceChanceAsync(Chat chat, double choiceChance)
        {
            if ((choiceChance < 0.0) || (choiceChance > 1.0))
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
            string text = turn == null ? "Игра закончена" : turn.GetMassage();
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