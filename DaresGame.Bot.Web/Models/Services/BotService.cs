using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DaresGame.Bot.Web.Models.Commands;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace DaresGame.Bot.Web.Models.Services
{
    internal class BotService : IBotService, IHostedService
    {
        public TelegramBotClient Client { get; }
        public IReadOnlyList<Command> Commands { get; }
        public Settings Settings { get; }

        private readonly BotConfiguration _config;

        public BotService(IOptions<BotConfiguration> options)
        {
            _config = options.Value;

            Client = new TelegramBotClient(_config.Token);

            Settings = new Settings(_config.InitialPlayersAmount, _config.InitialChoiceChance, _config.Decks);

            var commands = new List<Command>
            {
                new NewCommand(Settings),
                new DrawCommand(Settings)
            };

            Commands = commands.AsReadOnly();
            var startCommand = new StartCommand(Commands, _config.ManualLines, _config.AdditionalCommandsLines,
                _config.Host, Settings);

            commands.Insert(0, startCommand);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Client.SetWebhookAsync(_config.Url, cancellationToken: cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Client.DeleteWebhookAsync(cancellationToken);
    }
}