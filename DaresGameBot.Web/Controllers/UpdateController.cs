using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DaresGameBot.Web.Models;
using DaresGameBot.Web.Models.Commands;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DaresGameBot.Web.Controllers
{
    public sealed class UpdateController : Controller
    {
        private readonly IBot _botService;

        public UpdateController(IBot botService) => _botService = botService;

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
                        await GameLogic.ChangePlayersAmountAsync(playersAmount, _botService.Settings,
                            _botService.Client, message.Chat);
                    }

                    if (float.TryParse(message.Text, NumberStyles.Any, CultureInfo.InvariantCulture,
                        out float choiceChance))
                    {
                        await GameLogic.ChangeChoiceChanceAsync(choiceChance, _botService.Settings, _botService.Client,
                            message.Chat);
                    }
                }
            }

            return Ok();
        }
    }
}
