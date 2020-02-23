using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DaresGame.Bot.Web.Models.Services
{
    public interface IBotService
    {
        TelegramBotClient Client { get; }

        Task OnMessageReceivedAsync(Message message);
    }
}