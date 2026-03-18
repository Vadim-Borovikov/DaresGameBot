using System;
using System.Threading.Tasks;
using AbstractBot.Models.Operations;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations;

internal sealed class DrawCard : Operation
{
    public override Enum AccessRequired => Bot.AccessType.Admin;

    public DrawCard(Bot bot) : base(bot.Core.Accesses, bot.Core.UpdateSender) => _bot = bot;

    protected override bool IsInvokingBy(Message message, User? sender, string callbackQueryDataCore) => true;

    protected override Task ExecuteAsync(Message message, User sender, string callbackQueryDataCore)
    {
        return _bot.DrawCardAsync();
    }

    private readonly Bot _bot;
}