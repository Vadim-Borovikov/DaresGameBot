using System.Threading.Tasks;
using GoogleSheetsManager;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DaresGameBot.Web.Models.Commands
{
    internal sealed class DrawCommand : Command
    {
        internal override string Name => "draw";
        internal override string Description => Caption.ToLowerInvariant();

        protected override string Caption => GameLogic.DrawCaption;

        public DrawCommand(Config.Config config, Provider googleSheetsProvider)
        {
            _config = config;
            _googleSheetsProvider = googleSheetsProvider;
        }

        internal override Task ExecuteAsync(ChatId chatId, int replyToMessageId, ITelegramBotClient client)
        {
            return GamesRepository.DrawAsync(_config, _googleSheetsProvider, client, chatId, replyToMessageId);
        }

        private readonly Config.Config _config;
        private readonly Provider _googleSheetsProvider;
    }
}
