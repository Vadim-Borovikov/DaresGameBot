using System.Threading.Tasks;
using AbstractBot.Operations;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations;

internal sealed class StartGameWithPersonalPreferences : OperationSimple
{
    public StartGameWithPersonalPreferences(Bot bot) : base(bot) => _bot = bot;

    protected override bool IsInvokingBy(Message message, User sender) => false;

    protected override bool IsInvokingBy(Message message, User sender, string callbackQueryDataCore) => true;

    protected override Task ExecuteAsync(Message message, User sender)
    {
        return _bot.StartGameWithPersonalPreferences(message.Chat, sender);
    }

    private readonly Bot _bot;
}