using System.Threading.Tasks;
using DaresGameBot.Game;
using GoogleSheetsManager;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DaresGameBot.Bot.Commands
{
    internal sealed class NewCommand : Command
    {
        internal override string Name => "new";
        internal override string Description => Caption.ToLowerInvariant();
        protected override string Caption => Logic.NewGameCaption;

        public NewCommand(Config config, Provider googleSheetsProvider)
        {
            _config = config;
            _googleSheetsProvider = googleSheetsProvider;
        }

        public override Task ExecuteAsync(ChatId chatId, int replyToMessageId, ITelegramBotClient client)
        {
            return Repository.StartNewGameAsync(_config, _googleSheetsProvider, client, chatId, replyToMessageId);
        }

        private readonly Config _config;
        private readonly Provider _googleSheetsProvider;
    }
}
