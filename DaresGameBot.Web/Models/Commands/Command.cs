using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DaresGameBot.Web.Models.Commands
{
    public abstract class Command
    {
        internal abstract string Name { get; }
        internal abstract string Description { get; }

        protected virtual string Caption => null;

        internal bool IsInvokingBy(Message message)
        {
            return (message.Type == MessageType.Text) &&
                   ((message.Text == $"/{Name}") ||
                    (!string.IsNullOrWhiteSpace(Caption) && (message.Text == Caption)));
        }

        internal abstract Task ExecuteAsync(ChatId chatId, ITelegramBotClient client);
    }
}
