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
        internal override string Description => "список команд";

        private readonly IReadOnlyList<Command> _commands;
        private readonly string _url;
        private readonly Settings _settings;

        public StartCommand(IReadOnlyList<Command> commands, string url, Settings settings)
        {
            _commands = commands;
            _url = url;
            _settings = settings;
        }

        internal override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Привет!");
            builder.AppendLine();
            foreach (Command command in _commands)
            {
                builder.AppendLine($"/{command.Name} – {command.Description}");
            }
            builder.AppendLine("целое число – изменить количество игроков");
            builder.AppendLine("дробное число от 0.0 до 1.0 – изменить шанс на 🤩");
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
