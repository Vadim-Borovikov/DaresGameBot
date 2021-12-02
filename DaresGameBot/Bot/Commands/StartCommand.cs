using System.Threading.Tasks;
using AbstractBot;
using DaresGameBot.Game;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DaresGameBot.Bot.Commands
{
    internal sealed class StartCommand : CommandBase<Bot, BotConfig>
    {
        protected override string Name => "start";
        protected override string Description => "инструкции и команды";

        public StartCommand(Bot bot) : base(bot) { }

        public override async Task ExecuteAsync(Message message, bool fromChat = false)
        {
            await Bot.Client.SendTextMessageAsync(message.Chat, Bot.GetDescriptionFor(message.From.Id), ParseMode.MarkdownV2);

            if (!Manager.IsGameManagerValid(message.Chat.Id))
            {
                await Manager.StartNewGameAsync(Bot, message.Chat.Id);
            }
        }
    }
}
