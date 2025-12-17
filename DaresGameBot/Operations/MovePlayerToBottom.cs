using AbstractBot.Models.Operations;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations;

internal sealed class MovePlayerToBottom : Operation<string>
{
    public override Enum AccessRequired => Bot.AccessType.Admin;

    public MovePlayerToBottom(Bot bot) : base(bot.Core.Accesses, bot.Core.UpdateSender) => _bot = bot;

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

    protected override Task ExecuteAsync(string id, Message message, User sender) => _bot.MovePlayerDown(id, true);

    private readonly Bot _bot;
}