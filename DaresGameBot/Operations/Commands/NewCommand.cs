using System.Threading.Tasks;
using DaresGameBot.Operations.Data.GameButtons;
using Telegram.Bot.Types;
using System;
using AbstractBot.Models.Operations.Commands;
using AbstractBot.Interfaces.Modules.Config;

namespace DaresGameBot.Operations.Commands;

internal sealed class NewCommand : Command
{
    public override Enum AccessRequired => Bot.AccessType.Admin;

    public NewCommand(Bot bot, ITexts texts)
        : base(bot.Core.Accesses, bot.Core.UpdateSender, "new", texts, bot.Core.SelfUsername)
    {
        _bot = bot;
    }

    protected override Task ExecuteAsync(Message message, User sender)
    {
        return _bot.OnEndGameRequestedAsync(ConfirmEndData.ActionAfterGameEnds.StartNewGame);
    }

    private readonly Bot _bot;
}