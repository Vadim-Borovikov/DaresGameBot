using System.Threading.Tasks;
using DaresGameBot.Game;
using Telegram.Bot.Types;

namespace DaresGameBot.Commands;

internal sealed class DrawActionCommand : CommandWithAlias
{
    protected override string Alias => Game.Game.DrawActionCaption;

    public DrawActionCommand(Bot bot) : base(bot, "action", Game.Game.DrawActionCaption.ToLowerInvariant()) { }

    public override Task ExecuteAsync(Message message, Chat chat, string? payload)
    {
        int replyToMessageId = AbstractBot.Utils.IsGroup(chat) ? message.MessageId : 0;
        return Manager.DrawAsync(Bot, chat, replyToMessageId);
    }
}