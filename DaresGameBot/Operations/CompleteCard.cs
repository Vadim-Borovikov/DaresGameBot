using System;
using System.Threading.Tasks;
using AbstractBot.Bots;
using AbstractBot.Operations;
using DaresGameBot.Operations.Data.GameButtons;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations;

internal sealed class CompleteCard : Operation<CompleteCardData>
{
    protected override byte Order => 8;

    public override Enum AccessRequired => Bot.AccessType.Admin;

    public CompleteCard(Bot bot) => _bot = bot;

    protected override bool IsInvokingBy(User self, Message message, User sender, out CompleteCardData? data)
    {
        data = null;
        return false;
    }

    protected override bool IsInvokingBy(User self, Message message, User sender, string callbackQueryDataCore,
        out CompleteCardData? data)
    {
        data = CompleteCardData.From(callbackQueryDataCore);
        return data is not null;
    }

    protected override Task ExecuteAsync(BotBasic bot, CompleteCardData data, Message message, User sender)
    {
        return _bot.CompleteCardAsync(data);
    }

    private readonly Bot _bot;
}