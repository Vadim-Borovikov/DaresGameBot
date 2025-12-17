using AbstractBot.Models.Operations;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations;

internal sealed class MovePlayerDown : Operation<string>
{
    public override Enum AccessRequired => Bot.AccessType.Admin;

    public MovePlayerDown(Bot bot) : base(bot.Core.Accesses, bot.Core.UpdateSender) => _bot = bot;

    protected override bool IsInvokingBy(Message message, User sender, out string? name)
    {
        name = null;
        return false;
    }

    protected override bool IsInvokingBy(Message message, User sender, string callbackQueryDataCore,
        out string? name)
    {
        name = callbackQueryDataCore;
        return true;
    }

    protected override Task ExecuteAsync(string name, Message message, User sender) => _bot.MovePlayerDown(name);

    private readonly Bot _bot;
}