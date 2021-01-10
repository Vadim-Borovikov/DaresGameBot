using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DaresGameBot.Web.Models.Commands
{
    internal sealed class StartCommand : Command
    {
        internal override string Name => "start";
        internal override string Description => "инструкция и список команд";

        private readonly IReadOnlyCollection<Command> _commands;
        private readonly List<string> _manualLines;
        private readonly List<string> _additionalCommandsLines;
        private readonly Settings _settings;

        public StartCommand(IReadOnlyCollection<Command> commands, List<string> manualLines,
            List<string> additionalCommandsLines, Settings settings)
        {
            _commands = commands;
            _manualLines = manualLines;
            _additionalCommandsLines = additionalCommandsLines;
            _settings = settings;
        }

        internal override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            var builder = new StringBuilder();
            foreach (string line in _manualLines)
            {
                builder.AppendLine(line);
            }
            builder.AppendLine();
            builder.AppendLine("Команды:");
            foreach (Command command in _commands)
            {
                builder.AppendLine($"/{command.Name} – {command.Description}");
            }
            foreach (string line in _additionalCommandsLines)
            {
                builder.AppendLine(line);
            }

            await client.SendTextMessageAsync(message.Chat, builder.ToString());

            if (!GameLogic.IsGameValid(message.Chat))
            {
                await GameLogic.StartNewGameAsync(_settings.InitialPlayersAmount, _settings.InitialChoiceChance,
                    _settings.Decks, client, message.Chat);
            }
        }
    }
}
