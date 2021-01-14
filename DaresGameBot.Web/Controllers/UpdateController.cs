using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DaresGameBot.Web.Models;
using DaresGameBot.Web.Models.Commands;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace DaresGameBot.Web.Controllers
{
    public sealed class UpdateController : Controller
    {
        public UpdateController(IBot bot)
        {
            _bot = bot;
            _dontUnderstandSticker = new InputOnlineFile(_bot.Config.DontUnderstandStickerFileId);
        }

        public async Task<OkResult> Post([FromBody]Update update)
        {
            await ProcessAsync(update);
            return Ok();
        }

        private async Task ProcessAsync(Update update)
        {
            if (update?.Type != UpdateType.Message)
            {
                return;
            }

            Message message = update.Message;
            bool fromChat = message.Chat.Id != message.From.Id;
            string botName = fromChat ? await _bot.Client.GetNameAsync() : null;

            int replyToMessageId = fromChat ? message.MessageId : 0;

            Command command = _bot.Commands.FirstOrDefault(c => c.IsInvokingBy(message, fromChat, botName));
            if (command != null)
            {
                await command.ExecuteAsync(message.Chat.Id, replyToMessageId, _bot.Client);
                return;
            }

            if (ushort.TryParse(message.Text, out ushort playersAmount))
            {
                await GamesRepository.ChangePlayersAmountAsync(playersAmount, _bot.Config, _bot.GoogleSheetsProvider,
                    _bot.Client, message.Chat, replyToMessageId);
                return;
            }

            if (float.TryParse(message.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out float choiceChance))
            {
                await GamesRepository.ChangeChoiceChanceAsync(choiceChance, _bot.Config, _bot.GoogleSheetsProvider,
                    _bot.Client, message.Chat, replyToMessageId);
                return;
            }

            await _bot.Client.SendStickerAsync(message, _dontUnderstandSticker);
        }

        private readonly IBot _bot;
        private readonly InputOnlineFile _dontUnderstandSticker;
    }
}
