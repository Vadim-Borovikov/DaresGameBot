using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GoogleSheetsManager;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DaresGameBot.Web.Models.Commands
{
    internal sealed class StartCommand : Command
    {
        internal override string Name => "start";
        internal override string Description => "инструкция и список команд";

        public StartCommand(IReadOnlyCollection<Command> commands, List<string> manualLines,
            List<string> additionalCommandsLines, Config.Config config, Provider googleSheetsProvider)
        {
            _commands = commands;
            _manualLines = manualLines;
            _additionalCommandsLines = additionalCommandsLines;
            _config = config;
            _googleSheetsProvider = googleSheetsProvider;
        }

        internal override async Task ExecuteAsync(ChatId chatId, int replyToMessageId, ITelegramBotClient client)
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

            await client.SendTextMessageAsync(chatId, builder.ToString());

            if (!GamesRepository.IsGameValid(chatId))
            {
                await GamesRepository.StartNewGameAsync(_config, _googleSheetsProvider, client, chatId,
                    replyToMessageId);
            }
        }

        private readonly IReadOnlyCollection<Command> _commands;
        private readonly List<string> _manualLines;
        private readonly List<string> _additionalCommandsLines;
        private readonly Config.Config _config;
        private readonly Provider _googleSheetsProvider;
    }
}
