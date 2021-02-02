using System.Threading.Tasks;
using DaresGameBot.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace DaresGameBot.Web.Controllers
{
    public sealed class UpdateController : Controller
    {
        public async Task<OkResult> Post([FromBody]Update update, [FromServices]BotSingleton singleton)
        {
            await singleton.Bot.UpdateAsync(update);
            return Ok();
        }
    }
}
