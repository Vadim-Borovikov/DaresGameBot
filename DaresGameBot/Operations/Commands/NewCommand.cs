﻿using AbstractBot.Operations.Commands;
using System.Threading.Tasks;
using DaresGameBot.Operations.Data.GameButtons;
using Telegram.Bot.Types;
using System;
using AbstractBot.Bots;

namespace DaresGameBot.Operations.Commands;

internal sealed class NewCommand : CommandSimple
{
    protected override byte Order => 2;

    public override Enum AccessRequired => Bot.AccessType.Admin;

    public NewCommand(Bot bot)
        : base(bot.Config.Texts.CommandDescriptionFormat, "new", bot.Config.Texts.NewGameCaption)
    {
        _bot = bot;
    }

    protected override Task ExecuteAsync(BotBasic bot, Message message, User sender)
    {
        return _bot.OnEndGameRequestedAsync(ConfirmEndData.ActionAfterGameEnds.StartNewGame);
    }

    private readonly Bot _bot;
}