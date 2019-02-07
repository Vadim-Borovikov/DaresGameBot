using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using DaresGame;
using Telegram.Bot.Types;
using File = System.IO.File;

namespace DaresGameBot.Console
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            string token = ConfigurationManager.AppSettings.Get("token");
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new Exception("No token");
            }

            string choiceChanceValue = ConfigurationManager.AppSettings.Get("choiceChance");
            if (!double.TryParse(choiceChanceValue, NumberStyles.Any, CultureInfo.InvariantCulture,
                out double choiceChance))
            {
                throw new Exception("No choice chance");
            }

            string initialPlayersNumberValue = ConfigurationManager.AppSettings.Get("initialPlayersNumber");
            if (!int.TryParse(initialPlayersNumberValue, out int initialPlayersNumber))
            {
                throw new Exception("No initial players number");
            }

            string decksPath = ConfigurationManager.AppSettings.Get("decksPath");
            IEnumerable<Deck> decks = InitializeDecks(decksPath);

            var botLogic = new BotLogc(token, initialPlayersNumber, choiceChance, decks);

            User me = botLogic.Bot.GetMeAsync().Result;
            System.Console.Title = me.Username;

            botLogic.Bot.StartReceiving();
            System.Console.WriteLine($"Start listening for @{me.Username}");
            System.Console.ReadLine();
            botLogic.Bot.StopReceiving();
        }

        private static IEnumerable<Deck> InitializeDecks(string path)
        {
            return Directory.EnumerateFiles(path).OrderBy(p => p).Select(InitializeDeck);
        }

        private static Deck InitializeDeck(string path)
        {
            string[] lines = File.ReadAllLines(path);
            if (lines.Length < 2)
            {
                throw new Exception("Incorrect deck");
            }

            string tag = lines[0];

            var cards = new List<Card>();
            for (int i = 1; i < lines.Length; ++i)
            {
                string line = lines[i];
                Card card = InitializeCard(line);
                cards.Add(card);
            }

            return new Deck(tag, cards);
        }

        private static Card InitializeCard(string line)
        {
            int colonIndex = line.IndexOf(':');
            string parntersChunk = line.Substring(0, colonIndex);
            if (!int.TryParse(parntersChunk, out int partnersNumber))
            {
                throw new Exception($"Incorrect card: {line}");
            }

            string description = line.Substring(colonIndex + 1);

            return new Card(description, partnersNumber);
        }
    }
}
