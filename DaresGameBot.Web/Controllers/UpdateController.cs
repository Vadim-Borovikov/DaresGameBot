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
        public UpdateController(IBot bot) => _bot = bot;

        [HttpPost]
        public async Task<OkResult> Post([FromBody]Update update)
        {
            if (update?.Type == UpdateType.Message)
            {
                Message message = update.Message;

                string botName = null;
                bool fromChat = message.Chat.Id != message.From.Id;
                if (fromChat)
                {
                    User me = await _bot.Client.GetMeAsync();
                    botName = me.Username;
                }

                await GetAction(message, fromChat, botName);
            }

            return Ok();
        }

        private Task GetAction(Message message, bool fromChat, string botName)
        {
            int replyToMessageId = fromChat ? message.MessageId : 0;

            Command command = _bot.Commands.FirstOrDefault(c => c.IsInvokingBy(message, fromChat, botName));
            if (command != null)
            {
                return command.ExecuteAsync(message.Chat.Id, replyToMessageId, _bot.Client);
            }

            if (ushort.TryParse(message.Text, out ushort playersAmount))
            {
                return GamesRepository.ChangePlayersAmountAsync(playersAmount, _bot.Config, _bot.GoogleSheetsProvider,
                    _bot.Client, message.Chat, replyToMessageId);
            }

            return float.TryParse(message.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out float choiceChance)
                ? GamesRepository.ChangeChoiceChanceAsync(choiceChance, _bot.Config, _bot.GoogleSheetsProvider,
                    _bot.Client, message.Chat, replyToMessageId)
                : Task.CompletedTask;
        }

        private readonly IBot _bot;
    }
}
