using System.Threading.Tasks;
using AbstractBot.Operations;
using DaresGameBot.Operations.Data.GameButtons;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations;

internal sealed class RevealCard : Operation<RevealCardData>
{
    protected override byte Order => 6;

    public RevealCard(Bot bot) : base(bot) => _bot = bot;

    protected override bool IsInvokingBy(Message message, User sender, out RevealCardData? data)
    {
        data = null;
        return false;
    }

    protected override bool IsInvokingBy(Message message, User sender, string callbackQueryDataCore,
        out RevealCardData? data)
    {
        data = RevealCardData.From(callbackQueryDataCore);
        return data is not null;
    }

    protected override Task ExecuteAsync(RevealCardData data, Message message, User sender)
    {
        return _bot.RevealCardAsync(message.Chat, message.MessageId, sender, data);
    }

    private readonly Bot _bot;
}