using System.Threading.Tasks;
using DaresGameBot.Game;
using Telegram.Bot.Types;

namespace DaresGameBot.Bot.Commands
{
    internal sealed class NewCommand : Command
    {
        protected override string Name => "new";
        protected override string Description => Alias.ToLowerInvariant();

        protected override string Alias => Logic.NewGameCaption;

        public NewCommand(Bot bot) : base(bot) { }

        public override Task ExecuteAsync(Message message, bool fromChat = false)
        {
            return Repository.StartNewGameAsync(Bot.Config, GoogleSheetsProvider, Bot.Client, message.Chat);
        }
    }
}
