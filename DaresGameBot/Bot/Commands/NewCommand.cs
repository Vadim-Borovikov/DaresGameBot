using System.Threading.Tasks;
using AbstractBot;
using DaresGameBot.Game;
using Telegram.Bot.Types;

namespace DaresGameBot.Bot.Commands
{
    internal sealed class NewCommand : CommandBase<Bot, BotConfig>
    {
        protected override string Name => "new";
        protected override string Description => Alias.ToLowerInvariant();

        protected override string Alias => Manager.NewGameCaption;

        public NewCommand(Bot bot) : base(bot) { }

        public override Task ExecuteAsync(Message message, bool fromChat = false)
        {
            return Repository.StartNewGameAsync(Bot, message.Chat);
        }
    }
}
