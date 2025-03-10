using System;
using System.Threading.Tasks;
using AbstractBot.Interfaces.Modules;
using AbstractBot.Interfaces.Modules.Config;
using AbstractBot.Models.Operations.Commands;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations.Commands;

internal sealed class LangCommand : Command
{
    public override Enum AccessRequired => Bot.AccessType.Admin;

    public LangCommand(Bot bot, ITextsProvider<ITexts> textsProvider)
        : base(bot.Core.Accesses, bot.Core.UpdateSender, "lang", textsProvider, bot.Core.SelfUsername)
    {
        _bot = bot;
    }

    protected override Task ExecuteAsync(Message message, User sender) => _bot.OnToggleLanguagesAsync(sender);

    private readonly Bot _bot;
}