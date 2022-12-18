﻿using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DaresGameBot.Commands;

internal sealed class DrawActionCommand : DaresGameCommand
{
    protected override byte MenuOrder => 3;

    protected override string Alias => Game.Game.DrawActionCaption;

    public DrawActionCommand(Bot bot) : base(bot, "action", Game.Game.DrawActionCaption.ToLowerInvariant()) { }

    protected override Task ExecuteAsync(Chat chat, int replyToMessageId)
    {
        return GameManager.DrawAsync(chat, replyToMessageId);
    }
}