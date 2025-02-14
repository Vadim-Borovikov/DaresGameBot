using AbstractBot.Operations.Commands;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations.Commands;

internal sealed class RatesCommand : CommandSimple
{
    protected override byte Order => 3;

    public RatesCommand(Bot bot) : base(bot, "rates", bot.Config.Texts.ShowRatesCaption) => _bot = bot;

    protected override Task ExecuteAsync(Message message, User sender) => _bot.ShowRatesAsync(message.Chat, sender);

    private readonly Bot _bot;
}