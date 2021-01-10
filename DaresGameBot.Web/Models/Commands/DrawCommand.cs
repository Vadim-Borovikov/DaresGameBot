using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DaresGameBot.Web.Models.Commands
{
    internal class DrawCommand : Command
    {
        internal override string Name => "draw";
        internal override string Description => Caption.ToLowerInvariant();

        public const string Caption = "Вытянуть фант";

        private readonly Settings _settings;

        public DrawCommand(Settings settings)
        {
            _settings = settings;
        }

        internal override bool Contains(Message message)
        {
            return (message.Type == MessageType.Text)
                && (message.Text.Contains(Name) || message.Text.Contains(Caption));
        }

        internal override Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            return GameLogic.DrawAsync(_settings, client, message.Chat);
        }
    }
}
