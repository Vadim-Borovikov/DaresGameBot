using System.Threading.Tasks;
using AbstractBot.Commands;
using DaresGameBot.Game;
using Telegram.Bot.Types;

namespace DaresGameBot.Commands;

internal sealed class DrawActionCommand : CommandBase<Bot, Config>
{
    protected override string Alias => Game.Game.DrawActionCaption;

    public DrawActionCommand(Bot bot) : base(bot, "action", Game.Game.DrawActionCaption.ToLowerInvariant()) { }

    public override Task ExecuteAsync(Message message, bool fromChat, string? payload)
    {
        int replyToMessageId = fromChat ? message.MessageId : 0;
        return Manager.DrawAsync(Bot, message.Chat, replyToMessageId);
    }
}