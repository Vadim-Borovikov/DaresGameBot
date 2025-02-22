using System;
using System.Threading.Tasks;
using AbstractBot.Bots;
using AbstractBot.Operations.Commands;
using DaresGameBot.Operations.Data.GameButtons;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations.Commands;

internal sealed class UpdateCommand : CommandSimple
{
    protected override byte Order => 4;

    public override Enum AccessRequired => Bot.AccessType.Admin;

    public UpdateCommand(Bot bot)
        : base(bot.Config.Texts.CommandDescriptionFormat, "update", bot.Config.Texts.UpdateCommandDescription)
    {
        _bot = bot;
    }

    protected override Task ExecuteAsync(BotBasic bot, Message message, User sender)
    {
        return _bot.OnEndGameRequestedAsync(ConfirmEndData.ActionAfterGameEnds.UpdateCards);
    }

    private readonly Bot _bot;
}