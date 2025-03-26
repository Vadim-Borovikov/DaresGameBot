using System;
using System.Threading.Tasks;
using AbstractBot.Models.Operations;
using DaresGameBot.Operations.Data.GameButtons;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations;

internal sealed class CompleteCard : Operation<CompleteCardData>
{
    public override Enum AccessRequired => Bot.AccessType.Admin;

    public CompleteCard(Bot bot) : base(bot.Core.Accesses, bot.Core.UpdateSender) => _bot = bot;

    protected override bool IsInvokingBy(Message message, User sender, out CompleteCardData? data)
    {
        data = null;
        return false;
    }

    protected override bool IsInvokingBy(Message message, User sender, string callbackQueryDataCore,
        out CompleteCardData? data)
    {
        data = CompleteCardData.From(message.Text, callbackQueryDataCore);
        return data is not null;
    }

    protected override Task ExecuteAsync(CompleteCardData data, Message message, User sender)
    {
        return _bot.CompleteCardAsync(data);
    }

    private readonly Bot _bot;
}