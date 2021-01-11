using System.Collections.Generic;
using DaresGameBot.Web.Models.Commands;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace DaresGameBot.Web.Models
{
    internal sealed class Bot : IBot
    {
        public TelegramBotClient Client { get; }

        public IReadOnlyCollection<Command> Commands => _commands.AsReadOnly();

        public Config.Config Config { get; }

        public Bot(IOptions<Config.Config> options)
        {
            Config = options.Value;

            Client = new TelegramBotClient(Config.Token);
        }

        public void InitCommands()
        {
            _commands = new List<Command>
            {
                new NewCommand(Config.Settings),
                new DrawCommand(Config.Settings)
            };

            var startCommand =
                new StartCommand(Commands, Config.ManualLines, Config.AdditionalCommandsLines, Config.Settings);

            _commands.Insert(0, startCommand);
        }

        private List<Command> _commands;
    }
}