using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DaresGameBot.Web.Models.Commands
{
    internal sealed class NewCommand : Command
    {
        internal override string Name => "new";
        internal override string Description => Caption.ToLowerInvariant();

        public const string Caption = "Новая игра";

        private readonly Settings _settings;

        public NewCommand(Settings settings) => _settings = settings;

        internal override bool Contains(Message message)
        {
            return (message.Type == MessageType.Text)
                && (message.Text.Contains(Name) || message.Text.Contains(Caption));
        }

        internal override Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            return GameLogic.StartNewGameAsync(_settings.InitialPlayersAmount, _settings.InitialChoiceChance,
                _settings.Decks, client, message.Chat);
        }
    }
}
