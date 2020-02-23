using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DaresGame.Logic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using File = System.IO.File;

namespace DaresGame.Bot.Web.Models.Services
{
    internal class BotService : IBotService, IHostedService
    {
        public TelegramBotClient Client { get; }

        private readonly BotConfiguration _config;

        private readonly BotLogic _botLogic;

        public BotService(IOptions<BotConfiguration> options)
        {
            _config = options.Value;

            IEnumerable<Deck> decks = InitializeDecks(_config.DecksFolderPath);

            Client = new TelegramBotClient(_config.Token);

            _botLogic = new BotLogic(Client, _config.InitialPlayersNumber, _config.ChoiceChance, decks);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Client.SetWebhookAsync(_config.Url, cancellationToken: cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Client.DeleteWebhookAsync(cancellationToken);

        public Task OnMessageReceivedAsync(Message message) => _botLogic.OnMessageReceivedAsync(message);

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