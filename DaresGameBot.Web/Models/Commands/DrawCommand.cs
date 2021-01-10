using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DaresGameBot.Web.Models.Commands
{
    internal sealed class DrawCommand : Command
    {
        internal override string Name => "draw";
        internal override string Description => Caption.ToLowerInvariant();
        protected override string Caption => GameLogic.DrawCaption;

        private readonly Settings _settings;

        public DrawCommand(Settings settings) => _settings = settings;

        internal override Task ExecuteAsync(ChatId chatId, ITelegramBotClient client)
        {
            return GameLogic.DrawAsync(_settings, client, chatId);
        }
    }
}
