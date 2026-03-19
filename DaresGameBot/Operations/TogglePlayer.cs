using AbstractBot.Models.Operations;
using System;
using System.Threading.Tasks;
using GoogleSheetsManager.Extensions;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations;

internal sealed class TogglePlayer : Operation<long>
{
    public override Enum AccessRequired => Bot.AccessType.Admin;

    public TogglePlayer(Bot bot) : base(bot.Core.Accesses, bot.Core.UpdateSender) => _bot = bot;

    protected override bool IsInvokingBy(Message message, User? sender, out long id)
    {
        id = default;
        return false;
    }

    protected override bool IsInvokingBy(Message message, User? sender, string callbackQueryDataCore,
        out long id)
    {
        id = default;
        long? parsed = callbackQueryDataCore.ToLong();
        if (parsed is null)
        {
            return false;
        }
        id = parsed.Value;
        return true;
    }

    protected override Task ExecuteAsync(long id, Message message, User sender)
    {
        return _bot.TogglePlayerAsync(id);
    }

    private readonly Bot _bot;
}