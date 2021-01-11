using System.Collections.Generic;
using DaresGameBot.Web.Models.Commands;
using GoogleSheetsManager;
using Telegram.Bot;

namespace DaresGameBot.Web.Models
{
    public interface IBot
    {
        TelegramBotClient Client { get; }
        IReadOnlyCollection<Command> Commands { get; }
        Config.Config Config { get; }
        Provider GoogleSheetsProvider { get; }

        void Initialize(Provider googleSheetsProvider);
    }
}