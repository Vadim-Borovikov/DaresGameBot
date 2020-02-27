using System.Collections.Generic;
using DaresGame.Bot.Web.Models.Commands;
using Telegram.Bot;

namespace DaresGame.Bot.Web.Models.Services
{
    public interface IBotService
    {
        TelegramBotClient Client { get; }
        IReadOnlyList<Command> Commands { get; }
        Settings Settings { get; }
    }
}