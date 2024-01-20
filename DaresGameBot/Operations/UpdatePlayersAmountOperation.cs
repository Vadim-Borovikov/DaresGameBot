using System.Threading.Tasks;
using AbstractBot.Operations;
using DaresGameBot.Operations.Info;
using GoogleSheetsManager.Extensions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DaresGameBot.Operations;

internal sealed class UpdatePlayersAmountOperation : Operation<PlayersAmountInfo>
{
    protected override byte Order => 6;

    public UpdatePlayersAmountOperation(Bot bot) : base(bot, bot.Config.Texts.UpdatePlayersAmountOperationDescription)
    {
        _bot = bot;
    }

    protected override bool IsInvokingBy(Message message, User sender, out PlayersAmountInfo? data)
    {
        data = null;
        if (message.Type != MessageType.Text)
        {
            return false;
        }

        byte? amount = message.Text.ToByte();
        switch (amount)
        {
            case null:
            case < 2: return false;
            default:
                data = new PlayersAmountInfo(amount.Value);
                return true;
        }
    }

    protected override Task ExecuteAsync(PlayersAmountInfo data, Message message, User sender)
    {
        return _bot.Repository.UpdatePlayersAmountAsync(message.Chat, data.Amount);
    }

    private readonly Bot _bot;
}