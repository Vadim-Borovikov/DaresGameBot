using DaresGameBot.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace DaresGameBot.Web.Controllers;

[Route("")]
public sealed class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index([FromServices] BotSingleton singleton) => View(singleton.Bot.User);
}
