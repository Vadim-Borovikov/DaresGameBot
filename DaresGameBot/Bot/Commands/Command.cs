using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DaresGameBot.Bot.Commands
{
    internal abstract class Command
    {
        internal abstract string Name { get; }
        internal abstract string Description { get; }

        protected virtual string Caption => null;

        public bool IsInvokingBy(Message message, bool fromChat, string botName)
        {
            return (message.Type == MessageType.Text)
                   && ((message.Text == (fromChat ? $"/{Name}@{botName}" : $"/{Name}"))
                       || (!string.IsNullOrWhiteSpace(Caption) && (message.Text == Caption)));
        }

        public abstract Task ExecuteAsync(ChatId chatId, int replyToMessageId, ITelegramBotClient client);
    }
}
