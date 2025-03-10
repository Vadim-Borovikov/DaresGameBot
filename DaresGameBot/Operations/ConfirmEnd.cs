using System;
using System.Threading.Tasks;
using AbstractBot.Models.Operations;
using DaresGameBot.Operations.Data.GameButtons;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations;

internal sealed class ConfirmEnd : Operation<ConfirmEndData>
{
    public override Enum AccessRequired => Bot.AccessType.Admin;

    public ConfirmEnd(Bot bot) : base(bot.Core.Accesses, bot.Core.UpdateSender) => _bot = bot;

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
        return _bot.OnEndGameConfirmedAsync(data.After, sender);
    }

    private readonly Bot _bot;
}