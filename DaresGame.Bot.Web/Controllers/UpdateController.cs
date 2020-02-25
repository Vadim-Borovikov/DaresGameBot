using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DaresGame.Bot.Web.Models.Commands;
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

                Command command = _botService.Commands.FirstOrDefault(c => c.Contains(message));
                if (command != null)
                {
                    await command.ExecuteAsync(message, _botService.Client);
                }
                else
                {
                    if (int.TryParse(message.Text, out int playersAmount))
                    {
                        await _botService.GameLogic.ChangePlayersAmountAsync(message.Chat, playersAmount);
                    }

                    if (double.TryParse(message.Text, NumberStyles.Any, CultureInfo.InvariantCulture,
                        out double choiceChance))
                    {
                        await _botService.GameLogic.ChangeChoiceChanceAsync(message.Chat, choiceChance);
                    }
                }
            }

            return Ok();
        }
    }
}
