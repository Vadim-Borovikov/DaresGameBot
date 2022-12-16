using System.Threading.Tasks;
using DaresGameBot.Game;
using Telegram.Bot.Types;

namespace DaresGameBot.Commands;

internal sealed class DrawActionCommand : DaresGameCommand
{
    protected override int Priority => 3;

    protected override string Alias => Game.Game.DrawActionCaption;

    public DrawActionCommand(Bot bot) : base(bot, "action", Game.Game.DrawActionCaption.ToLowerInvariant()) { }

    protected override Task ExecuteAsync(Chat chat, int replyToMessageId)
    {
        return Manager.DrawAsync(Bot, chat, replyToMessageId);
    }
}