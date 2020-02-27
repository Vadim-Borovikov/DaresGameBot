using System.Collections.Concurrent;
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
    internal static class GameLogic
    {
        private static readonly ConcurrentDictionary<long, Game> Games = new ConcurrentDictionary<long, Game>();

        public static Task StartNewGameAsync(int initialPlayersAmount, float initialChoiceChance,
            IEnumerable<Deck> decks, ITelegramBotClient client, Chat chat)
        {
            var game = new Game(initialPlayersAmount, initialChoiceChance, decks);

            Games.AddOrUpdate(chat.Id, game, (id, g) => g);

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("🔥 Начинаем новую игру!");
            stringBuilder.AppendLine(game.Players);
            stringBuilder.AppendLine(game.Chance);
            return
                client.SendTextMessageAsync(chat, stringBuilder.ToString(), replyMarkup: GetKeyboard(IsValid(game)));
        }

        public static Task ChangePlayersAmountAsync(int playersAmount, Settings settings, ITelegramBotClient client,
            Chat chat)
        {
            if (playersAmount <= 0)
            {
                return Task.CompletedTask;
            }

            bool success = Games.TryGetValue(chat.Id, out Game game);
            if (!success)
            {
                return StartNewGameAsync(playersAmount, settings.InitialChoiceChance, settings.Decks, client, chat);
            }

            game.PlayersAmount = playersAmount;

            return
                client.SendTextMessageAsync(chat, $"Принято! {game.Players}", replyMarkup: GetKeyboard(IsValid(game)));
        }

        public static Task ChangeChoiceChanceAsync(float choiceChance, Settings settings, ITelegramBotClient client,
            Chat chat)
        {
            if ((choiceChance < 0.0f) || (choiceChance > 1.0f))
            {
                return Task.CompletedTask;
            }

            bool success = Games.TryGetValue(chat.Id, out Game game);
            if (!success)
            {
                return StartNewGameAsync(settings.InitialPlayersAmount, choiceChance, settings.Decks, client, chat);
            }

            game.ChoiceChance = choiceChance;

            return
                client.SendTextMessageAsync(chat, $"Принято! {game.Chance}", replyMarkup: GetKeyboard(IsValid(game)));
        }

        public static Task DrawAsync(Settings settings, ITelegramBotClient client, Chat chat)
        {
            bool success = Games.TryGetValue(chat.Id, out Game game);
            if (!success)
            {
                return StartNewGameAsync(settings.InitialPlayersAmount, settings.InitialChoiceChance, settings.Decks,
                    client, chat);
            }

            Turn turn = game?.Draw();
            string text = turn?.GetMessage(game.PlayersAmount) ?? "Игра закончена";
            return client.SendTextMessageAsync(chat, text, replyMarkup: GetKeyboard(IsValid(game)));
        }

        public static bool IsGameValid(Chat chat)
        {
            bool success = Games.TryGetValue(chat.Id, out Game game);
            return success && IsValid(game);
        }

        private static ReplyKeyboardMarkup GetKeyboard(bool draw)
        {
            string caption = draw ? DrawCommand.Caption : NewCommand.Caption;
            var button = new KeyboardButton(caption);
            var raw = new[] { button };
            return new ReplyKeyboardMarkup(raw, true);
        }

        private static bool IsValid(Game game) => (game != null) && !game.Empty;
    }
}