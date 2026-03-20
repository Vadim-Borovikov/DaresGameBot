using System;
using System.Threading.Tasks;
using AbstractBot.Models.Operations;
using GoogleSheetsManager.Extensions;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations;

internal sealed class SetupRound : Operation<byte>
{
    public override Enum AccessRequired => Bot.AccessType.Admin;

    public SetupRound(Bot bot) : base(bot.Core.Accesses, bot.Core.UpdateSender) => _bot = bot;

    protected override bool IsInvokingBy(Message message, User? sender, out byte round)
    {
        round = default;
        return false;
    }

    protected override bool IsInvokingBy(Message message, User? sender, string callbackQueryDataCore,
        out byte round)
    {
        byte? parced = callbackQueryDataCore.ToByte();
        if (parced is null)
        {
            round = default;
            return false;
        }
        round = parced.Value;
        return true;
    }

    protected override Task ExecuteAsync(byte round, Message message, User sender) => _bot.SetupRoundAsync(round);

    private readonly Bot _bot;
}