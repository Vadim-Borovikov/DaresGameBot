using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AbstractBot.Models.Operations;
using DaresGameBot.Configs;
using DaresGameBot.Operations.Data.PlayerListUpdates;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DaresGameBot.Operations;

internal sealed class UpdatePlayers : Operation<UpdatesData>
{
    public override Enum AccessRequired => Bot.AccessType.Admin;

    public UpdatePlayers(Bot bot, Texts texts) : base(bot.Core.Accesses, bot.Core.UpdateSender)
    {
        _bot = bot;
        _texts = texts;

        HelpDescription = texts.UpdatePlayersOperationDescription;
    }

    protected override bool IsInvokingBy(Message message, User sender, out UpdatesData? data)
    {
        data = null;
        if (message.Type != MessageType.Text)
        {
            return false;
        }

        List<string>? lines = message.Text?.Split(_texts.PlayersSeparator).Select(l => l.Trim()).ToList();

        switch (lines?.Count)
        {
            case null:
            case < 1: return false;
            default:
                data = UpdatesData.From(lines, _texts);
                return data is not null;
        }
    }

    protected override Task ExecuteAsync(UpdatesData data, Message message, User sender)
    {
        return _bot.CanBeUpdated()
            ? _bot.UpdatePlayersAsync(data.Datas)
            : _texts.Refuse.SendAsync(_bot.Core.UpdateSender, message.Chat);
    }

    private readonly Bot _bot;
    private readonly Texts _texts;
}