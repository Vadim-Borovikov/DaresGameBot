using AbstractBot.Models.Operations;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations;

internal sealed class AcceptPartnersGenders : Operation
{
    public override Enum AccessRequired => Bot.AccessType.Player;

    public AcceptPartnersGenders(Bot bot) : base(bot.Core.Accesses, bot.Core.UpdateSender)
    {
        _bot = bot;
    }

    protected override bool IsInvokingBy(Message message, User? sender) => false;

    protected override bool IsInvokingBy(Message message, User? sender, string callbackQueryDataCore)
    {
        return callbackQueryDataCore == string.Empty;
    }

    protected override Task ExecuteAsync(Message message, User sender)
    {
        return Task.CompletedTask; //_bot.AcceptPartnersGendersAsync(message.Chat, sender));
    }

    private readonly Bot _bot;
}