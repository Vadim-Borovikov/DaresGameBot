using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DaresGame.Bot.Web.Models.Commands
{
    internal class StartCommand : Command
    {
        internal override string Name => "start";
        internal override string Description => "инструкция и список команд";

        private readonly IReadOnlyList<Command> _commands;
        private readonly List<string> _manualLines;
        private readonly List<string> _additionalCommandsLines;
        private readonly string _url;
        private readonly Settings _settings;

        public StartCommand(IReadOnlyList<Command> commands, List<string> manualLines,
            List<string> additionalCommandsLines, string url, Settings settings)
        {
            _commands = commands;
            _manualLines = manualLines;
            _additionalCommandsLines = additionalCommandsLines;
            _url = url;
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
            builder.AppendLine();
            builder.AppendLine($"Иногда я засыпаю, но ты можешь меня разбудить, если зайдёшь на {_url}.");

            await client.SendTextMessageAsync(message.Chat, builder.ToString());

            if (!GameLogic.IsGameValid(message.Chat))
            {
                await GameLogic.StartNewGameAsync(_settings.InitialPlayersAmount, _settings.InitialChoiceChance,
                    _settings.Decks, client, message.Chat);
            }
        }
    }
}
