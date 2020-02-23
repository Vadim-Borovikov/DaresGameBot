using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DaresGame.Logic;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Game = DaresGame.Logic.Game;

namespace DaresGame.Bot.Web.Models
{
    internal class BotLogic
    {
        private readonly TelegramBotClient _client;

        private Game _game;
        private bool IsGameValid => (_game != null) && !_game.Empty;
        private readonly Settings _settings;

        private const string StartCommand = "/start";
        private const string NewGameCommand = "Новая игра";
        private const string DrawCommand = "Вытянуть фант";
        private const string ResetComand = "/reset";

        private static readonly string Info =
            $"Бот для игры в фанты Каддла+.{Environment.NewLine}" +
            $"Партнёры — ваши соседи слева. 🤩 — произвольный выбор.{Environment.NewLine}" +
            $"/start — эта инструкция{Environment.NewLine}" +
            $"/reset — запуск новой игры{Environment.NewLine}" +
            $"целое число — изменить число игроков{Environment.NewLine}" +
            "дробное число от 0.0 до 1.0 — изменить шанс на 🤩";

        public BotLogic(TelegramBotClient client, int playersNumber, double choiceChance, IEnumerable<Deck> decks)
        {
            _settings = new Settings
            {
                PlayersNumber = playersNumber,
                ChoiceChance = choiceChance,
                Decks = decks
            };

            _client = client;
        }

        public Task OnMessageReceivedAsync(Message message)
        {
            Console.WriteLine(message.Text);

            switch (message.Text)
            {
                case StartCommand:
                    return OnStartCommand(message.Chat.Id);
                case DrawCommand:
                    return OnDrawCommand(message.Chat.Id);
                case NewGameCommand:
                case ResetComand:
                    return StartNewGame(message.Chat.Id);
            }

            if (int.TryParse(message.Text, out int playersNumber))
            {
                return OnNewPlayerNumberCommand(message.Chat.Id, playersNumber);
            }

            if (double.TryParse(message.Text, NumberStyles.Any, CultureInfo.InvariantCulture,
                out double choiceChance))
            {
                return OnNewChoiceChanceCommand(message.Chat.Id, choiceChance);
            }

            return Task.CompletedTask;
        }

        private async Task OnStartCommand(long chatId)
        {
            await SendTextMessageAsync(chatId, Info);
            if (!IsGameValid)
            {
                await StartNewGame(chatId);
            }
        }

        private async Task OnDrawCommand(long chatId)
        {
            if (!IsGameValid)
            {
                await StartNewGame(chatId);
                return;
            }

            Turn turn = _game?.Draw();
            string message = turn == null ? "Игра закончена" : GetMessage(turn);
            await SendTextMessageAsync(chatId, message);
        }

        private async Task StartNewGame(long chatId)
        {
            _game = new Game(_settings);

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("🔥 Начинаем новую игру!");
            stringBuilder.AppendLine($"Игроков: {_settings.PlayersNumber}");
            stringBuilder.AppendLine($"Шанс на 🤩: {_settings.ChoiceChance:P0}");
            await SendTextMessageAsync(chatId, stringBuilder.ToString());
        }

        private async Task OnNewPlayerNumberCommand(long chatId, int playerNumber)
        {
            if (playerNumber <= 0)
            {
                return;
            }

            _settings.PlayersNumber = playerNumber;

            if (!IsGameValid)
            {
                await StartNewGame(chatId);
                return;
            }

            _game.UpdateSettings(_settings);
            await SendTextMessageAsync(chatId, $"Игроков: {_settings.PlayersNumber}");
        }

        private async Task OnNewChoiceChanceCommand(long chatId, double choiceChance)
        {
            if ((choiceChance < 0.0) || (choiceChance > 1.0))
            {
                return;
            }

            _settings.ChoiceChance = choiceChance;

            if (!IsGameValid)
            {
                await StartNewGame(chatId);
                return;
            }

            _game.UpdateSettings(_settings);
            await SendTextMessageAsync(chatId, $"Шанс на 🤩: {_settings.ChoiceChance:P0}");
        }

        private static string GetMessage(Turn turn)
        {
            var builder = new StringBuilder();

            builder.Append(turn.Text);

            if (turn.Partners.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine();
                builder.Append(turn.Partners.Count > 1 ? "Партнёры: " : "Партнёр: ");
                IEnumerable<string> parnters =
                    turn.Partners.Select(p => p.ByChoice ? "🤩" : p.PartnerNumber.ToString());
                builder.Append(string.Join(", ", parnters));
            }

            return builder.ToString();
        }

        private async Task SendTextMessageAsync(long chatId, string message)
        {
            string command = (_game != null) && !_game.Empty ? DrawCommand : NewGameCommand;
            var replyKeyboard = new ReplyKeyboardMarkup(new[] { new KeyboardButton(command) }, true);
            await _client.SendTextMessageAsync(chatId, message, replyMarkup: replyKeyboard);
        }
    }
}