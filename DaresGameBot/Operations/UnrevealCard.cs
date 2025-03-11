using System;
using System.Threading.Tasks;
using AbstractBot.Models.Operations;
using DaresGameBot.Operations.Data.GameButtons;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations;

internal sealed class UnrevealCard : Operation<UnervealCardData>
{
    public override Enum AccessRequired => Bot.AccessType.Admin;

    public UnrevealCard(Bot bot) : base(bot.Core.Accesses, bot.Core.UpdateSender) => _bot = bot;

    protected override bool IsInvokingBy(Message message, User sender, out UnervealCardData? data)
    {
        data = null;
        return false;
    }

    protected override bool IsInvokingBy(Message message, User sender, string callbackQueryDataCore,
        out UnervealCardData? data)
    {
        data = UnervealCardData.From(callbackQueryDataCore);
        return data is not null;
    }

    protected override Task ExecuteAsync(UnervealCardData data, Message message, User sender)
    {
        return _bot.UnrevealCardAsync(message.MessageId, data, sender.Id);
    }

    private readonly Bot _bot;
}