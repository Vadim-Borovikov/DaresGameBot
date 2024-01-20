using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations.Commands;

internal sealed class NewCommand : DaresGameCommand
{
    protected override byte Order => 2;

    protected override string Alias => _bot.Config.Texts.NewGameCaption;

    public NewCommand(Bot bot) : base(bot, "new", bot.Config.Texts.NewGameCaption.ToLowerInvariant()) => _bot = bot;

    protected override Task ExecuteAsync(Chat chat, int _) => GameManager.StartNewGameAsync(chat);

    private readonly Bot _bot;
}