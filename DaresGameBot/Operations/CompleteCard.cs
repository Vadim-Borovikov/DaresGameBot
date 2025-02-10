using System.Threading.Tasks;
using AbstractBot.Operations;
using DaresGameBot.Operations.Data.GameButtons;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations;

internal sealed class CompleteCard : Operation<GameButtonData>
{
    protected override byte Order => 4;

    public CompleteCard(Bot bot) : base(bot) => _bot = bot;

    protected override bool IsInvokingBy(Message message, User sender, out GameButtonData? data)
    {
        data = null;
        return false;
    }

    protected override bool IsInvokingBy(Message message, User sender, string callbackQueryDataCore,
        out GameButtonData? data)
    {
        data = GameButtonData.From(callbackQueryDataCore);
        return data is not null;
    }

    protected override Task ExecuteAsync(GameButtonData data, Message message, User sender)
    {
        return _bot.CompleteCardAsync(message.Chat, sender, data);
    }

    private readonly Bot _bot;
}