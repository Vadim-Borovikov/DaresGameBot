using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations.Commands;

internal sealed class NewCommand : DaresGameCommand
{
    protected override byte Order => 3;

    protected override string Alias => _bot.Config.Texts.NewGameCaption;

    public NewCommand(Bot bot) : base(bot, "new", bot.Config.Texts.NewGameCaption.ToLowerInvariant()) => _bot = bot;

    protected override Task ExecuteAsync(Chat chat, User sender, int replyToMessageId)
    {
        return _bot.OnNewGameAsync(chat, sender);
    }

    private readonly Bot _bot;
}