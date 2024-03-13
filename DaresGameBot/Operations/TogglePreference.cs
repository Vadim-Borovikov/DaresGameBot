using System.Threading.Tasks;
using AbstractBot.Operations;
using DaresGameBot.Operations.Info;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations;

internal sealed class TogglePreference : Operation<TogglePreferenceInfo>
{
    public TogglePreference(Bot bot) : base(bot) => _bot = bot;

    protected override bool IsInvokingBy(Message message, User sender, out TogglePreferenceInfo? info)
    {
        info = null;
        return false;
    }

    protected override bool IsInvokingBy(Message message, User sender, string callbackQueryDataCore,
        out TogglePreferenceInfo? info)
    {
        info = TogglePreferenceInfo.From(callbackQueryDataCore);
        return info is not null;
    }

    protected override Task ExecuteAsync(TogglePreferenceInfo data, Message message, User sender)
    {
        return _bot.TogglePreferenceAsync(message.Chat, data.PartnerId);
    }

    private readonly Bot _bot;
}