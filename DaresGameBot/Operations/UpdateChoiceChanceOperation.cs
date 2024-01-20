using System.Threading.Tasks;
using AbstractBot.Operations;
using DaresGameBot.Operations.Info;
using GoogleSheetsManager.Extensions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DaresGameBot.Operations;

internal sealed class UpdateChoiceChanceOperation : Operation<ChoiceChanceInfo>
{
    protected override byte Order => 6;

    public UpdateChoiceChanceOperation(Bot bot)
        : base(bot, bot.Config.Texts.UpdateChoiceChanceOperationDescriptionFormat.Format(bot.Config.Texts.Choosable))
    {
        _bot = bot;
    }

    protected override bool IsInvokingBy(Message message, User sender, out ChoiceChanceInfo? data)
    {
        data = null;
        if (message.Type != MessageType.Text)
        {
            return false;
        }

        decimal? chance = message.Text.ToDecimal();
        switch (chance)
        {
            case null:
            case < 0.0m or > 1.0m: return false;
            default:
                data = new ChoiceChanceInfo(chance.Value);
                return true;
        }
    }

    protected override Task ExecuteAsync(ChoiceChanceInfo data, Message message, User sender)
    {
        return _bot.UpdateChoiceChanceAsync(message.Chat, data.Chance);
    }

    private readonly Bot _bot;
}