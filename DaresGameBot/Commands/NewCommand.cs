﻿using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DaresGameBot.Commands;

internal sealed class NewCommand : DaresGameCommand
{
    protected override byte MenuOrder => 2;

    protected override string Alias => Game.Game.NewGameCaption;

    public NewCommand(Bot bot) : base(bot, "new", Game.Game.NewGameCaption.ToLowerInvariant()) { }

    protected override Task ExecuteAsync(Chat chat, int _) => GameManager.StartNewGameAsync(chat);
}