using System.Threading.Tasks;
using AbstractBot.Operations;
using DaresGameBot.Operations.Info;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations;

internal sealed class RevealCard : Operation<GameButtonInfo>
{
    protected override byte Order => 10;

    public RevealCard(Bot bot) : base(bot)
    {
        _bot = bot;
    }

    protected override bool IsInvokingBy(Message message, User sender, out GameButtonInfo? info)
    {
        info = null;
        return false;
    }

    protected override bool IsInvokingBy(Message message, User sender, string callbackQueryDataCore,
        out GameButtonInfo? info)
    {
        info = GameButtonInfo.From(callbackQueryDataCore);
        return info is not null;
    }

    protected override Task ExecuteAsync(GameButtonInfo info, Message message, User sender)
    {
        return _bot.RevealCardAsync(message.Chat, message.MessageId, sender, info);
    }

    private readonly Bot _bot;
}