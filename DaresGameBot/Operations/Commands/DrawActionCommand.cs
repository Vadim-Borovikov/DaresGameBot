using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations.Commands;

internal sealed class DrawActionCommand : DaresGameCommand
{
    protected override byte Order => 4;

    protected override string Alias => _bot.Config.Texts.DrawActionCaption;

    public DrawActionCommand(Bot bot) : base(bot, "action", bot.Config.Texts.DrawActionCaption.ToLowerInvariant())
    {
        _bot = bot;
    }

    protected override Task ExecuteAsync(Chat chat, User sender, int replyToMessageId) => _bot.DrawAsync(chat, sender, replyToMessageId);

    private readonly Bot _bot;
}