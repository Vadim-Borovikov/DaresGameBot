using System;
using System.Threading.Tasks;
using AbstractBot.Bots;
using AbstractBot.Operations;
using DaresGameBot.Operations.Data.GameButtons;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations;

internal sealed class RevealCard : Operation<RevealCardData>
{
    protected override byte Order => 7;

    public override Enum AccessRequired => Bot.AccessType.Player;

    public RevealCard(Bot bot) => _bot = bot;

    protected override bool IsInvokingBy(User self, Message message, User sender, out RevealCardData? data)
    {
        data = null;
        return false;
    }

    protected override bool IsInvokingBy(User self, Message message, User sender, string callbackQueryDataCore,
        out RevealCardData? data)
    {
        data = null;
        if (_bot.Players is not null)
        {
            data = RevealCardData.From(callbackQueryDataCore, _bot.Players);
        }
        return data is not null;
    }

    protected override Task ExecuteAsync(BotBasic bot, RevealCardData data, Message message, User sender)
    {
        return _bot.RevealCardAsync(message.MessageId, data);
    }

    private readonly Bot _bot;
}