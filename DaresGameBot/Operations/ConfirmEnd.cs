using System.Threading.Tasks;
using AbstractBot.Bots;
using AbstractBot.Operations;
using DaresGameBot.Operations.Data.GameButtons;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations;

internal sealed class ConfirmEnd : Operation<ConfirmEndData>
{
    protected override byte Order => 9;

    public ConfirmEnd(Bot bot) => _bot = bot;

    protected override bool IsInvokingBy(User self, Message message, User sender, out ConfirmEndData? data)
    {
        data = null;
        return false;
    }

    protected override bool IsInvokingBy(User self, Message message, User sender, string callbackQueryDataCore,
        out ConfirmEndData? data)
    {
        data = ConfirmEndData.From(callbackQueryDataCore);
        return data is not null;
    }

    protected override Task ExecuteAsync(BotBasic bot, ConfirmEndData data, Message message, User sender)
    {
        return _bot.OnEndGameConfirmedAsync(message.Chat, data.After);
    }

    private readonly Bot _bot;
}