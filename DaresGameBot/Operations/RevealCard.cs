using System;
using System.Threading.Tasks;
using AbstractBot.Models.Operations;
using DaresGameBot.Operations.Data.GameButtons;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations;

internal sealed class RevealCard : Operation<RevealCardData>
{
    public override Enum AccessRequired => Bot.AccessType.Player;

    public RevealCard(Bot bot) : base(bot.Core.Accesses, bot.Core.UpdateSender) => _bot = bot;

    protected override bool IsInvokingBy(Message message, User sender, out RevealCardData? data)
    {
        data = null;
        return false;
    }

    protected override bool IsInvokingBy(Message message, User sender, string callbackQueryDataCore,
        out RevealCardData? data)
    {
        data = new RevealCardData(callbackQueryDataCore);
        return true;
    }

    protected override Task ExecuteAsync(RevealCardData data, Message message, User sender)
    {
        return _bot.RevealCardAsync(data);
    }

    private readonly Bot _bot;
}