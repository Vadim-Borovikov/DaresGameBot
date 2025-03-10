using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using AbstractBot.Models.Operations.Commands;
using AbstractBot.Interfaces.Modules.Config;

namespace DaresGameBot.Operations.Commands;

internal sealed class RatesCommand : Command
{
    public override Enum AccessRequired => Bot.AccessType.Admin;

    public RatesCommand(Bot bot, ITexts texts)
        : base(bot.Core.Accesses, bot.Core.UpdateSender, "rates", texts, bot.Core.SelfUsername)
    {
        _bot = bot;
    }

    protected override Task ExecuteAsync(Message message, User sender) => _bot.ShowRatesAsync();

    private readonly Bot _bot;
}