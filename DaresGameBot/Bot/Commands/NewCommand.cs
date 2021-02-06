using System.Threading.Tasks;
using AbstractBot;
using DaresGameBot.Game;
using GoogleSheetsManager;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DaresGameBot.Bot.Commands
{
    internal sealed class NewCommand : CommandBase
    {
        protected override string Name => "new";
        protected override string Description => Alias.ToLowerInvariant();

        protected override string Alias => Logic.NewGameCaption;

        public NewCommand(BotConfig config, Provider googleSheetsProvider)
        {
            _config = config;
            _googleSheetsProvider = googleSheetsProvider;
        }

        public override Task ExecuteAsync(ChatId chatId, ITelegramBotClient client, int replyToMessageId = 0,
            IReplyMarkup replyMarkup = null)
        {
            return Repository.StartNewGameAsync(_config, _googleSheetsProvider, client, chatId, replyToMessageId);
        }

        private readonly BotConfig _config;
        private readonly Provider _googleSheetsProvider;
    }
}
