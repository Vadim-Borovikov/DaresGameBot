using AbstractBot;
using GoogleSheetsManager;

namespace DaresGameBot.Bot.Commands
{
    internal abstract class Command : CommandBase<BotConfig>
    {
        protected Command(Bot bot) : base(bot) => GoogleSheetsProvider = bot.GoogleSheetsProvider;

        protected readonly Provider GoogleSheetsProvider;
    }
}
