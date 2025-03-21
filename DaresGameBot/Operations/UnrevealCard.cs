using System;
using System.Threading.Tasks;
using AbstractBot.Bots;
using AbstractBot.Operations;
using DaresGameBot.Operations.Data.GameButtons;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations;

internal sealed class UnrevealCard : Operation<UnervealCardData>
{
    protected override byte Order => 8;

    public override Enum AccessRequired => Bot.AccessType.Admin;

    public UnrevealCard(Bot bot) => _bot = bot;

    protected override bool IsInvokingBy(User self, Message message, User sender, out UnervealCardData? data)
    {
        data = null;
        return false;
    }

    protected override bool IsInvokingBy(User self, Message message, User sender, string callbackQueryDataCore,
        out UnervealCardData? data)
    {
        data = null;
        if (_bot.Players is not null)
        {
            data = UnervealCardData.From(callbackQueryDataCore, _bot.Players);
        }
        return data is not null;
    }

    protected override Task ExecuteAsync(BotBasic bot, UnervealCardData data, Message message, User sender)
    {
        return _bot.UnrevealCardAsync(message.MessageId, data);
    }

    private readonly Bot _bot;
}