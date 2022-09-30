using DaresGameBot.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace DaresGameBot.Web.Controllers;

public sealed class UpdateController : Controller
{
    public OkResult Post([FromServices] BotSingleton singleton, [FromBody] Update update)
    {
        singleton.Bot.Update(update);
        return Ok();
    }
}