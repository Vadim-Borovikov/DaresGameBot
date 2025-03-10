using System.Threading.Tasks;
using DaresGameBot.Operations.Data.GameButtons;
using Telegram.Bot.Types;
using System;
using AbstractBot.Models.Operations.Commands;
using AbstractBot.Interfaces.Modules.Config;
using AbstractBot.Interfaces.Modules;

namespace DaresGameBot.Operations.Commands;

internal sealed class NewCommand : Command
{
    public override Enum AccessRequired => Bot.AccessType.Admin;

    public NewCommand(Bot bot, ITextsProvider<ITexts> textsProvider)
        : base(bot.Core.Accesses, bot.Core.UpdateSender, "new", textsProvider, bot.Core.SelfUsername)
    {
        _bot = bot;
    }

    protected override Task ExecuteAsync(Message message, User sender)
    {
        return _bot.OnEndGameRequestedAsync(ConfirmEndData.ActionAfterGameEnds.StartNewGame, sender);
    }

    private readonly Bot _bot;
}