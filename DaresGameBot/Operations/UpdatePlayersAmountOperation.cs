using System.Threading.Tasks;
using AbstractBot.Operations;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DaresGameBot.Operations;

internal sealed class UpdatePlayersAmountOperation : Operation
{
    protected override byte MenuOrder => 5;

    public UpdatePlayersAmountOperation(Bot bot) : base(bot)
    {
        MenuDescription = "*целое число* – изменить количество игроков";
        _bot = bot;
    }

    protected override async Task<ExecutionResult> TryExecuteAsync(Message message, long senderId)
    {
        if (message.Type != MessageType.Text)
        {
            return ExecutionResult.UnsuitableOperation;
        }

        bool parsed = ushort.TryParse(message.Text, out ushort playersAmount);
        if (!parsed)
        {
            return ExecutionResult.UnsuitableOperation;
        }

        if (!IsAccessSuffice(senderId))
        {
            return ExecutionResult.InsufficentAccess;
        }

        bool success = await _bot.GameManager.UpdatePlayersAmountAsync(playersAmount, message.Chat);
        return success ? ExecutionResult.UnsuitableOperation : ExecutionResult.Success;
    }

    private readonly Bot _bot;
}