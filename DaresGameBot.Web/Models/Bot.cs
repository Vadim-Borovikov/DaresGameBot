using System.Collections.Generic;
using DaresGameBot.Web.Models.Commands;
using GoogleSheetsManager;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace DaresGameBot.Web.Models
{
    internal sealed class Bot : IBot
    {
        public TelegramBotClient Client { get; }

        public IReadOnlyCollection<Command> Commands => _commands.AsReadOnly();

        public Config.Config Config { get; }
        public Provider GoogleSheetsProvider { get; private set; }

        public Bot(IOptions<Config.Config> options)
        {
            Config = options.Value;

            Client = new TelegramBotClient(Config.Token);
        }

        public void Initialize(Provider googleSheetsProvider)
        {
            GoogleSheetsProvider = googleSheetsProvider;

            _commands = new List<Command>
            {
                new NewCommand(Config, GoogleSheetsProvider),
                new DrawCommand(Config, GoogleSheetsProvider)
            };

            var startCommand = new StartCommand(Commands, Config.ManualLines, Config.AdditionalCommandsLines, Config,
                GoogleSheetsProvider);

            _commands.Insert(0, startCommand);
        }

        private List<Command> _commands;
    }
}