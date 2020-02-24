using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DaresGame.Bot.Web.Models.Commands
{
    public abstract class Command
    {
        internal abstract string Name { get; }
        internal abstract string Description { get; }

        internal virtual bool Contains(Message message)
        {
            return (message.Type == MessageType.Text) && message.Text.Contains(Name);
        }

        internal abstract Task ExecuteAsync(Message message, ITelegramBotClient client);
    }
}
