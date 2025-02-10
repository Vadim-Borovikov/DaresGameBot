using AbstractBot.Operations.Commands;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations.Commands;

internal sealed class NewCommand : CommandSimple
{
    protected override byte Order => 2;

    public NewCommand(Bot bot) : base(bot, "new", bot.Config.Texts.NewGameCaption) => _bot = bot;

    protected override Task ExecuteAsync(Message message, User sender) => _bot.OnNewGameAsync(message.Chat, sender);

    private readonly Bot _bot;
}