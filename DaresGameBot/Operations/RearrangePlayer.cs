using System;
using System.Threading.Tasks;
using AbstractBot.Models.Operations;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations;

internal sealed class RearrangePlayer : Operation<string>
{
    public override Enum AccessRequired => Bot.AccessType.Admin;

    public RearrangePlayer(Bot bot) : base(bot.Core.Accesses, bot.Core.UpdateSender) => _bot = bot;

    protected override bool IsInvokingBy(Message message, User sender, out string? name)
    {
        name = null;
        return false;
    }

    protected override bool IsInvokingBy(Message message, User sender, string callbackQueryDataCore,
        out string? id)
    {
        id = callbackQueryDataCore;
        return true;
    }

    protected override Task ExecuteAsync(string id, Message message, User sender) => _bot.RearrangePlayerAsync(id);

    private readonly Bot _bot;
}