using System;
using System.Threading.Tasks;
using AbstractBot.Interfaces.Modules.Config;
using AbstractBot.Models.Operations.Commands;
using DaresGameBot.Operations.Data.GameButtons;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations.Commands;

internal sealed class UpdateCommand : Command
{
    public override Enum AccessRequired => Bot.AccessType.Admin;

    public UpdateCommand(Bot bot, ITexts texts)
        : base(bot.Core.Accesses, bot.Core.UpdateSender, "update", texts, bot.Core.SelfUsername)
    {
        _bot = bot;
    }

    protected override Task ExecuteAsync(Message message, User sender)
    {
        return _bot.OnEndGameRequestedAsync(ConfirmEndData.ActionAfterGameEnds.UpdateCards);
    }

    private readonly Bot _bot;
}