using System.Collections.Generic;
using DaresGame.Bot.Web.Models.Commands;
using Telegram.Bot;

namespace DaresGame.Bot.Web.Models
{
    public interface IBot
    {
        TelegramBotClient Client { get; }
        IReadOnlyCollection<Command> Commands { get; }
        Config.Config Config { get; }
        Settings Settings { get; }

        void InitCommands();
    }
}