using System.Threading.Tasks;
using DaresGameBot.Game;
using GoogleSheetsManager;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DaresGameBot.Bot.Commands
{
    internal sealed class DrawCommand : Command
    {
        internal override string Name => "draw";
        internal override string Description => Caption.ToLowerInvariant();

        protected override string Caption => Logic.DrawCaption;

        public DrawCommand(Config config, Provider googleSheetsProvider)
        {
            _config = config;
            _googleSheetsProvider = googleSheetsProvider;
        }

        public override Task ExecuteAsync(ChatId chatId, int replyToMessageId, ITelegramBotClient client)
        {
            return Repository.DrawAsync(_config, _googleSheetsProvider, client, chatId, replyToMessageId);
        }

        private readonly Config _config;
        private readonly Provider _googleSheetsProvider;
    }
}
