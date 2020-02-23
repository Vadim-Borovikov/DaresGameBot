using System.Threading.Tasks;
using DaresGame.Bot.Web.Models.Services;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DaresGame.Bot.Web.Controllers
{
    public class UpdateController : Controller
    {
        private readonly IBotService _botService;

        public UpdateController(IBotService botService) { _botService = botService; }

        [HttpPost]
        public async Task<OkResult> Post([FromBody]Update update)
        {
            if (update?.Type == UpdateType.Message)
            {
                Message message = update.Message;
                await _botService.OnMessageReceivedAsync(message);
            }

            return Ok();
        }
    }
}
