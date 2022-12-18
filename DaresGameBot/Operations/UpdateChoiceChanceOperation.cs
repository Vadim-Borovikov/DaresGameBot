using System.Globalization;
using System.Threading.Tasks;
using AbstractBot.Operations;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DaresGameBot.Operations;

internal sealed class UpdateChoiceChanceOperation : Operation
{
    protected override byte MenuOrder => 6;

    public UpdateChoiceChanceOperation(Bot bot) : base(bot)
    {
        MenuDescription =
            $"*{AbstractBot.Bots.Bot.EscapeCharacters("дробное число от 0.0 до 1.0")}* – изменить шанс на 🤩";
        _bot = bot;
    }

    protected override async Task<ExecutionResult> TryExecuteAsync(Message message, long senderId)
    {
        if (message.Type != MessageType.Text)
        {
            return ExecutionResult.UnsuitableOperation;
        }

        bool parsed =
            float.TryParse(message.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out float choiceChance);
        if (!parsed)
        {
            return ExecutionResult.UnsuitableOperation;
        }

        if (!IsAccessSuffice(senderId))
        {
            return ExecutionResult.InsufficentAccess;
        }

        bool success = await _bot.GameManager.UpdateChoiceChanceAsync(choiceChance, message.Chat);
        return success ? ExecutionResult.UnsuitableOperation : ExecutionResult.Success;
    }

    private readonly Bot _bot;
}