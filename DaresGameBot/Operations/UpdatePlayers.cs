using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AbstractBot.Operations;
using DaresGameBot.Operations.Data.PlayerListUpdates;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DaresGameBot.Operations;

internal sealed class UpdatePlayers : Operation<UpdatesData>
{
    protected override byte Order => 6;

    public UpdatePlayers(Bot bot) : base(bot, bot.Config.Texts.UpdatePlayersOperationDescription)
    {
        _bot = bot;
    }

    protected override bool IsInvokingBy(Message message, User sender, out UpdatesData? data)
    {
        data = null;
        if (message.Type != MessageType.Text)
        {
            return false;
        }

        List<string>? lines = message.Text?.Split(_bot.Config.Texts.PlayersSeparator).Select(l => l.Trim()).ToList();

        switch (lines?.Count)
        {
            case null:
            case < 1: return false;
            default:
                data = UpdatesData.From(lines, _bot.Config.Texts);
                return data is not null;
        }
    }

    protected override Task ExecuteAsync(UpdatesData data, Message message, User sender)
    {
        return _bot.CanBeUpdated(sender)
            ? _bot.UpdatePlayersAsync(message.Chat, sender, data.Datas)
            : _bot.Config.Texts.Refuse.SendAsync(_bot, message.Chat);
    }

    private readonly Bot _bot;
}