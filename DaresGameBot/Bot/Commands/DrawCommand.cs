using System.Threading.Tasks;
using AbstractBot;
using DaresGameBot.Game;
using Telegram.Bot.Types;

namespace DaresGameBot.Bot.Commands
{
    internal sealed class DrawCommand : CommandBase<Bot, BotConfig>
    {
        protected override string Name => "draw";
        protected override string Description => Alias.ToLowerInvariant();

        protected override string Alias => Manager.DrawCaption;

        public DrawCommand(Bot bot) : base(bot) { }

        public override Task ExecuteAsync(Message message, bool fromChat = false)
        {
            int replyToMessageId = fromChat ? message.MessageId : 0;
            return Repository.DrawAsync(Bot, message.Chat, replyToMessageId);
        }
    }
}
