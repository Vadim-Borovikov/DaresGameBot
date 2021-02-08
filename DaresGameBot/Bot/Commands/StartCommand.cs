using System.Threading.Tasks;
using DaresGameBot.Game;
using Telegram.Bot.Types;

namespace DaresGameBot.Bot.Commands
{
    internal sealed class StartCommand : Command
    {
        protected override string Name => "start";
        protected override string Description => "инструкции и команды";

        public StartCommand(Bot bot) : base(bot) { }

        public override async Task ExecuteAsync(Message message, bool fromChat = false)
        {
            await Bot.Client.SendTextMessageAsync(message.Chat, Bot.GetDescription());

            if (!Repository.IsGameValid(message.Chat))
            {
                await Repository.StartNewGameAsync(Bot.Config, GoogleSheetsProvider, Bot.Client, message.Chat);
            }
        }
    }
}
