using System.Threading.Tasks;
using AbstractBot;
using DaresGameBot.Game;
using Telegram.Bot.Types;

namespace DaresGameBot.Bot.Commands
{
    internal sealed class StartCommand : CommandBase<Bot, BotConfig>
    {
        protected override string Name => "start";
        protected override string Description => "инструкции и команды";

        public StartCommand(Bot bot) : base(bot) { }

        public override async Task ExecuteAsync(Message message, bool fromChat = false)
        {
            await Bot.Client.SendTextMessageAsync(message.Chat, Bot.GetDescription());

            if (!Repository.IsGameManagerValid(message.Chat))
            {
                await Repository.StartNewGameAsync(Bot, message.Chat);
            }
        }
    }
}
