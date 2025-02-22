using AbstractBot.Operations.Commands;
using System;
using System.Threading.Tasks;
using AbstractBot.Bots;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations.Commands;

internal sealed class RatesCommand : CommandSimple
{
    protected override byte Order => 3;

    public override Enum AccessRequired => Bot.AccessType.Admin;

    public RatesCommand(Bot bot)
        : base(bot.Config.Texts.CommandDescriptionFormat, "rates", bot.Config.Texts.ShowRatesCaption)
    {
        _bot = bot;
    }

    protected override Task ExecuteAsync(BotBasic bot, Message message, User sender) => _bot.ShowRatesAsync();

    private readonly Bot _bot;
}