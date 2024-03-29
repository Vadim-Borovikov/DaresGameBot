﻿using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations.Commands;

internal sealed class DrawQuestionCommand : DaresGameCommand
{
    protected override byte Order => 5;

    protected override string Alias => _bot.Config.Texts.DrawQuestionCaption;

    public DrawQuestionCommand(Bot bot)
        : base(bot, "question", bot.Config.Texts.DrawQuestionCaption.ToLowerInvariant())
    {
        _bot = bot;
    }

    protected override Task ExecuteAsync(Chat chat, User sender, int replyToMessageId)
    {
        return _bot.DrawAsync(chat, sender, replyToMessageId, false);
    }

    private readonly Bot _bot;
}