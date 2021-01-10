using System.Collections.Generic;
using DaresGame.Bot.Web.Models.Commands;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace DaresGame.Bot.Web.Models
{
    internal sealed class Bot : IBot
    {
        public TelegramBotClient Client { get; }

        public IReadOnlyCollection<Command> Commands => _commands.AsReadOnly();

        public Config.Config Config { get; }

        public Settings Settings { get; }

        public Bot(IOptions<Config.Config> options)
        {
            Config = options.Value;

            Client = new TelegramBotClient(Config.Token);

            Settings = new Settings(Config.InitialPlayersAmount, Config.InitialChoiceChance, Config.Decks);
        }

        public void InitCommands()
        {
            _commands = new List<Command>
            {
                new NewCommand(Settings),
                new DrawCommand(Settings)
            };

            var startCommand =
                new StartCommand(Commands, Config.ManualLines, Config.AdditionalCommandsLines, Settings);

            _commands.Insert(0, startCommand);
        }

        private List<Command> _commands;
    }
}