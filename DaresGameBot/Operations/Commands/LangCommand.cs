using System;
using System.Threading.Tasks;
using AbstractBot.Interfaces.Modules;
using AbstractBot.Interfaces.Modules.Config;
using AbstractBot.Models.Operations.Commands;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations.Commands;

internal sealed class LangCommand : Command
{
    public override Enum AccessRequired => Bot.AccessType.Player;

    public LangCommand(Bot bot, ITextsProvider<ITexts> textsProvider)
        : base(bot.Core.Accesses, bot.Core.UpdateSender, "lang", textsProvider, bot.Core.SelfUsername)
    {
        _bot = bot;
    }

    protected override Task ExecuteAsync(Message message, User sender)
    {
        return _bot.OnToggleLanguagesAsync(message.Chat, sender);
    }

    private readonly Bot _bot;
}