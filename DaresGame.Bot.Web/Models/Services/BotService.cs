using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DaresGame.Bot.Web.Models.Commands;
using DaresGame.Logic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace DaresGame.Bot.Web.Models.Services
{
    internal class BotService : IBotService, IHostedService
    {
        public TelegramBotClient Client { get; }
        public IReadOnlyList<Command> Commands { get; }
        public GameLogic GameLogic { get; }

        private readonly BotConfiguration _config;

        public BotService(IOptions<BotConfiguration> options)
        {
            _config = options.Value;

            Client = new TelegramBotClient(_config.Token);

            IEnumerable<Deck> decks = InitializeDecks(_config.DecksFolderPath);
            GameLogic = new GameLogic(Client, _config.InitialPlayersAmount, _config.ChoiceChance, decks);

            var commands = new List<Command>
            {
                new NewCommand(GameLogic),
                new DrawCommand(GameLogic)
            };

            Commands = commands.AsReadOnly();
            var startCommand = new StartCommand(Commands, _config.Host, GameLogic);

            commands.Insert(0, startCommand);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Client.SetWebhookAsync(_config.Url, cancellationToken: cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Client.DeleteWebhookAsync(cancellationToken);

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
            string[] chunks = line.Split(';');

            if (chunks.Length != 3)
            {
                throw new Exception($"Incorrect card: {line}");
            }

            if (!int.TryParse(chunks[0], out int players))
            {
                throw new Exception($"Incorrect card: {line}");
            }

            if (!int.TryParse(chunks[1], out int partnersToAssign))
            {
                throw new Exception($"Incorrect card: {line}");
            }

            string description = chunks[2];

            return new Card(description, players, partnersToAssign);
        }
    }
}