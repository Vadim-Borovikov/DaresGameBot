using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Reactive.Linq;
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

        private CancellationTokenSource _periodicCancellationSource;
        private Ping _ping;
        private readonly string _pingUrl;

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
                Settings);

            commands.Insert(0, startCommand);

            var uri = new Uri(_config.Url);
            _pingUrl = uri.Host;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _periodicCancellationSource = new CancellationTokenSource();
            _ping = new Ping();
            StartPeriodicPing(_periodicCancellationSource.Token);

            return Client.SetWebhookAsync(_config.Url, cancellationToken: cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _periodicCancellationSource.Cancel();
            _ping.Dispose();
            _periodicCancellationSource.Dispose();
            return Client.DeleteWebhookAsync(cancellationToken);
        }

        private void StartPeriodicPing(CancellationToken cancellationToken)
        {
            IObservable<long> observable = Observable.Interval(_config.PingPeriod);
            observable.Subscribe(PingSite, cancellationToken);
        }

        private void PingSite(long _) => _ping.Send(_pingUrl);
    }
}