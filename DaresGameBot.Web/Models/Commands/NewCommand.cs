using System.Threading.Tasks;
using DaresGameBot.Web.Models.Config;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DaresGameBot.Web.Models.Commands
{
    internal sealed class NewCommand : Command
    {
        internal override string Name => "new";
        internal override string Description => Caption.ToLowerInvariant();
        protected override string Caption => GameLogic.NewGameCaption;

        private readonly Settings _settings;

        public NewCommand(Settings settings) => _settings = settings;

        internal override Task ExecuteAsync(ChatId chatId, ITelegramBotClient client)
        {
            return GameLogic.StartNewGameAsync(_settings, client, chatId);
        }
    }
}
