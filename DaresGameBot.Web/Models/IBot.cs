using System.Collections.Generic;
using DaresGameBot.Web.Models.Commands;
using Telegram.Bot;

namespace DaresGameBot.Web.Models
{
    public interface IBot
    {
        TelegramBotClient Client { get; }
        IReadOnlyCollection<Command> Commands { get; }
        Config.Config Config { get; }

        void InitCommands();
    }
}