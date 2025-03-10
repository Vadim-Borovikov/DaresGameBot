using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace DaresGameBot.Web.Controllers;

public sealed class UpdateController : Controller
{
    public OkResult Post([FromServices] Bot bot, [FromBody] Update update)
    {
        bot.Update(update);
        return Ok();
    }
}