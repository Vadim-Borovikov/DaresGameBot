using System.Threading.Tasks;
using AbstractBot.Operations;
using DaresGameBot.Operations.Data.GameButtons;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations;

internal sealed class EndGame : Operation<EndGameData>
{
    protected override byte Order => 8;

    public EndGame(Bot bot) : base(bot) => _bot = bot;

    protected override bool IsInvokingBy(Message message, User sender, out EndGameData? data)
    {
        data = null;
        return false;
    }

    protected override bool IsInvokingBy(Message message, User sender, string callbackQueryDataCore,
        out EndGameData? data)
    {
        data = EndGameData.From(callbackQueryDataCore);
        return data is not null;
    }

    protected override Task ExecuteAsync(EndGameData data, Message message, User sender)
    {
        return _bot.OnEndGameConfirmedAsync(message.Chat, sender, data.After);
    }

    private readonly Bot _bot;
}