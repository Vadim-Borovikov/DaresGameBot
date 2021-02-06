using System.Threading.Tasks;
using AbstractBot;
using DaresGameBot.Game;
using GoogleSheetsManager;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DaresGameBot.Bot.Commands
{
    internal sealed class DrawCommand : CommandBase
    {
        protected override string Name => "draw";
        protected override string Description => Alias.ToLowerInvariant();

        protected override string Alias => Logic.DrawCaption;

        public DrawCommand(BotConfig config, Provider googleSheetsProvider)
        {
            _config = config;
            _googleSheetsProvider = googleSheetsProvider;
        }

        public override Task ExecuteAsync(ChatId chatId, ITelegramBotClient client, int replyToMessageId = 0,
            IReplyMarkup replyMarkup = null)
        {
            return Repository.DrawAsync(_config, _googleSheetsProvider, client, chatId, replyToMessageId);
        }

        private readonly BotConfig _config;
        private readonly Provider _googleSheetsProvider;
    }
}
