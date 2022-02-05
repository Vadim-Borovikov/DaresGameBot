using System.Threading.Tasks;
using AbstractBot;
using DaresGameBot.Game;
using Telegram.Bot.Types;

namespace DaresGameBot.Bot.Commands;

internal sealed class DrawActionCommand : CommandBase<Bot, BotConfig>
{
    protected override string Name => "action";
    protected override string Description => Alias.ToLowerInvariant();

    protected override string Alias => Game.Game.DrawActionCaption;

    public DrawActionCommand(Bot bot) : base(bot) { }

    public override Task ExecuteAsync(Message message, bool fromChat, string? payload)
    {
        int replyToMessageId = fromChat ? message.MessageId : 0;
        return Manager.DrawAsync(Bot, message.Chat.Id, replyToMessageId);
    }
}
