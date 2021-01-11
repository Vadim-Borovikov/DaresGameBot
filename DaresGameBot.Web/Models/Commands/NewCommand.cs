using System.Threading.Tasks;
using GoogleSheetsManager;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DaresGameBot.Web.Models.Commands
{
    internal sealed class NewCommand : Command
    {
        internal override string Name => "new";
        internal override string Description => Caption.ToLowerInvariant();
        protected override string Caption => GameLogic.NewGameCaption;

        public NewCommand(Config.Config config, Provider googleSheetsProvider)
        {
            _config = config;
            _googleSheetsProvider = googleSheetsProvider;
        }

        internal override Task ExecuteAsync(ChatId chatId, ITelegramBotClient client)
        {
            return GamesRepository.StartNewGameAsync(_config, _googleSheetsProvider, client, chatId);
        }

        private readonly Config.Config _config;
        private readonly Provider _googleSheetsProvider;
    }
}
