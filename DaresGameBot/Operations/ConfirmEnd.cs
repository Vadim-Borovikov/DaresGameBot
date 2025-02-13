using System.Threading.Tasks;
using AbstractBot.Operations;
using DaresGameBot.Operations.Data.GameButtons;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations;

internal sealed class ConfirmEnd : Operation<ConfirmEndData>
{
    protected override byte Order => 9;

    public ConfirmEnd(Bot bot) : base(bot) => _bot = bot;

    protected override bool IsInvokingBy(Message message, User sender, out ConfirmEndData? data)
    {
        data = null;
        return false;
    }

    protected override bool IsInvokingBy(Message message, User sender, string callbackQueryDataCore,
        out ConfirmEndData? data)
    {
        data = ConfirmEndData.From(callbackQueryDataCore);
        return data is not null;
    }

    protected override Task ExecuteAsync(ConfirmEndData data, Message message, User sender)
    {
        return _bot.OnEndGameConfirmedAsync(message.Chat, sender, data.After);
    }

    private readonly Bot _bot;
}