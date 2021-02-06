using System.Threading.Tasks;
using AbstractBot;
using DaresGameBot.Game;
using GoogleSheetsManager;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DaresGameBot.Bot.Commands
{
    internal sealed class StartCommand : CommandBase
    {
        protected override string Name => "start";
        protected override string Description => "инструкции и команды";

        public StartCommand(IDescriptionProvider descriptionProvider, BotConfig config, Provider googleSheetsProvider)
        {
            _descriptionProvider = descriptionProvider;
            _config = config;
            _googleSheetsProvider = googleSheetsProvider;
        }

        public override async Task ExecuteAsync(ChatId chatId, ITelegramBotClient client, int replyToMessageId = 0,
            IReplyMarkup replyMarkup = null)
        {
            await client.SendTextMessageAsync(chatId, _descriptionProvider.GetDescription(),
                replyToMessageId: replyToMessageId, replyMarkup: replyMarkup);

            if (!Repository.IsGameValid(chatId))
            {
                await Repository.StartNewGameAsync(_config, _googleSheetsProvider, client, chatId, replyToMessageId);
            }
        }

        private readonly IDescriptionProvider _descriptionProvider;
        private readonly BotConfig _config;
        private readonly Provider _googleSheetsProvider;
    }
}
