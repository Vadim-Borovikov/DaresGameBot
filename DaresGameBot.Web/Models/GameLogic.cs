using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DaresGameBot.Logic;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Game = DaresGameBot.Logic.Game;

namespace DaresGameBot.Web.Models
{
    internal static class GameLogic
    {
        public const string DrawCaption = "Вытянуть фант";
        public const string NewGameCaption = "Новая игра";

        private static readonly ConcurrentDictionary<long, Game> Games = new ConcurrentDictionary<long, Game>();

        public static Task StartNewGameAsync(ushort initialPlayersAmount, float initialChoiceChance,
            IEnumerable<Deck> decks, ITelegramBotClient client, ChatId chatId)
        {
            var game = new Game(initialPlayersAmount, initialChoiceChance, decks);

            Games.AddOrUpdate(chatId.Identifier, game, (id, g) => game);

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("🔥 Начинаем новую игру!");
            stringBuilder.AppendLine(game.Players);
            stringBuilder.AppendLine(game.Chance);
            return client.SendTextMessageAsync(chatId, stringBuilder.ToString(), replyMarkup: GetKeyboard(true));
        }

        public static Task ChangePlayersAmountAsync(ushort playersAmount, Settings settings, ITelegramBotClient client,
            ChatId chatId)
        {
            if (playersAmount <= 0)
            {
                return Task.CompletedTask;
            }

            bool success = IsGameValid(chatId, out Game game);
            if (!success)
            {
                return StartNewGameAsync(playersAmount, settings.InitialChoiceChance, settings.Decks, client, chatId);
            }

            game.PlayersAmount = playersAmount;

            return client.SendTextMessageAsync(chatId, $"Принято! {game.Players}", replyMarkup: GetKeyboard(game));
        }

        public static Task ChangeChoiceChanceAsync(float choiceChance, Settings settings, ITelegramBotClient client,
            ChatId chatId)
        {
            if ((choiceChance < 0.0f) || (choiceChance > 1.0f))
            {
                return Task.CompletedTask;
            }

            bool success = IsGameValid(chatId, out Game game);
            if (!success)
            {
                return StartNewGameAsync(settings.InitialPlayersAmount, choiceChance, settings.Decks, client, chatId);
            }

            game.ChoiceChance = choiceChance;

            return client.SendTextMessageAsync(chatId, $"Принято! {game.Chance}", replyMarkup: GetKeyboard(game));
        }

        public static Task DrawAsync(Settings settings, ITelegramBotClient client, ChatId chatId)
        {
            bool success = IsGameValid(chatId, out Game game);
            if (!success)
            {
                return StartNewGameAsync(settings.InitialPlayersAmount, settings.InitialChoiceChance, settings.Decks,
                    client, chatId);
            }

            Turn turn = game?.Draw();
            string text = turn?.GetMessage(game.PlayersAmount) ?? "Игра закончена";
            return client.SendTextMessageAsync(chatId, text, replyMarkup: GetKeyboard(game));
        }

        public static bool IsGameValid(ChatId chatId) => IsGameValid(chatId, out Game _);

        private static bool IsGameValid(ChatId chatId, out Game game)
        {
            bool success = Games.TryGetValue(chatId.Identifier, out game);
            return success && IsValid(game);
        }

        private static ReplyKeyboardMarkup GetKeyboard(Game game)
        {
            bool shouldDraw = IsValid(game);
            return GetKeyboard(shouldDraw);
        }

        private static ReplyKeyboardMarkup GetKeyboard(bool shouldDraw)
        {
            string caption = shouldDraw ? DrawCaption : NewGameCaption;
            var button = new KeyboardButton(caption);
            var raw = new[] { button };
            return new ReplyKeyboardMarkup(raw, true);
        }

        private static bool IsValid(Game game) => (game != null) && !game.Empty;
    }
}