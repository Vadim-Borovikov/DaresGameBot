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

    protected override Task ExecuteAsync(Chat chat, int replyToMessageId) => _bot.DrawAsync(chat, replyToMessageId);

    private readonly Bot _bot;
}