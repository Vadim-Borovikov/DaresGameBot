using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AbstractBot.Operations;
using DaresGameBot.Operations.Info;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DaresGameBot.Operations;

internal sealed class UpdatePlayers : Operation<UpdatesInfo>
{
    protected override byte Order => 6;

    public UpdatePlayers(Bot bot) : base(bot, bot.Config.Texts.UpdatePlayersOperationDescription)
    {
        _bot = bot;
    }

    protected override bool IsInvokingBy(Message message, User sender, out UpdatesInfo? info)
    {
        info = null;
        if (message.Type != MessageType.Text)
        {
            return false;
        }

        List<string>? lines = message.Text?.Split(PlayersSeparator).Select(l => l.Trim()).ToList();

        switch (lines?.Count)
        {
            case null:
            case < 1: return false;
            default:
                info = UpdatesInfo.From(lines);
                return info is not null;
        }
    }

    protected override Task ExecuteAsync(UpdatesInfo info, Message message, User sender)
    {
        return _bot.CanBeUpdated(sender)
            ? _bot.UpdatePlayersAsync(message.Chat, sender, info.Updates)
            : _bot.Config.Texts.Refuse.SendAsync(_bot, message.Chat);
    }

    private readonly Bot _bot;
    private const string PlayersSeparator = "\n";
}