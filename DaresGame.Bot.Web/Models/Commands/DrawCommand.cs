using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DaresGame.Bot.Web.Models.Commands
{
    internal class DrawCommand : Command
    {
        internal override string Name => "draw";
        internal override string Description => Caption.ToLowerInvariant();

        internal const string Caption = "Вытянуть фант";

        private readonly GameLogic _gameLogic;

        public DrawCommand(GameLogic gameLogic)
        {
            _gameLogic = gameLogic;
        }

        internal override bool Contains(Message message)
        {
            return (message.Type == MessageType.Text)
                && (message.Text.Contains(Name) || message.Text.Contains(Caption));
        }

        internal override Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            return _gameLogic.DrawAsync(message.Chat);
        }
    }
}
