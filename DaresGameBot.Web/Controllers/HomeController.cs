using System.Diagnostics;
using System.Threading.Tasks;
using DaresGameBot.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace DaresGameBot.Web.Controllers
{
    [Route("")]
    public sealed class HomeController : Controller
    {
        private readonly IBot _botService;

        public HomeController(IBot botService) { _botService = botService; }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index()
        {
            User model = await _botService.Client.GetMeAsync();
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var model = new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier };
            return View(model);
        }
    }
}
