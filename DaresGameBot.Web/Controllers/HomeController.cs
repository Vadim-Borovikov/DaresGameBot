using Microsoft.AspNetCore.Mvc;

namespace DaresGameBot.Web.Controllers;

[Route("")]
public sealed class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index([FromServices] Bot bot) => View(bot.Core.Self);
}