using System;
using System.Threading.Tasks;
using AbstractBot.Interfaces.Modules;
using AbstractBot.Interfaces.Modules.Config;
using AbstractBot.Models.Operations.Commands;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations.Commands;

internal sealed class ShowImagesCommand : Command
{
    public override Enum AccessRequired => Bot.AccessType.Admin;

    public ShowImagesCommand(Bot bot, ITextsProvider<ITexts> textsProvider)
        : base(bot.Core.Accesses, bot.Core.UpdateSender, "images", textsProvider, bot.Core.SelfUsername)
    {
        _bot = bot;
    }

    protected override Task ExecuteAsync(Message message, User sender) => _bot.ShowImagesAsync();

    private readonly Bot _bot;
}