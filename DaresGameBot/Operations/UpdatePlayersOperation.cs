using System.Threading.Tasks;
using AbstractBot.Operations;
using DaresGameBot.Operations.Info;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DaresGameBot.Operations;

internal sealed class UpdatePlayersOperation : Operation<PlayersInfo>
{
    protected override byte Order => 6;

    public UpdatePlayersOperation(Bot bot) : base(bot, bot.Config.Texts.UpdatePlayersOperationDescription)
    {
        _bot = bot;
    }

    protected override bool IsInvokingBy(Message message, User sender, out PlayersInfo? data)
    {
        data = null;
        if (message.Type != MessageType.Text)
        {
            return false;
        }

        string[]? parts = message.Text?.Split(PlayersSeparator);

        switch (parts?.Length)
        {
            case null:
            case < 2: return false;
            default:
                data = new PlayersInfo(parts);
                return true;
        }
    }

    protected override Task ExecuteAsync(PlayersInfo data, Message message, User sender)
    {
        return _bot.UpdatePlayersAsync(message.Chat, data.Names);
    }

    private readonly Bot _bot;

    private const string PlayersSeparator = "\n";
}