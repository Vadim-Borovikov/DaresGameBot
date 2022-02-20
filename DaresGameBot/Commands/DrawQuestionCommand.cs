using System.Threading.Tasks;
using AbstractBot;
using DaresGameBot.Game;
using Telegram.Bot.Types;

namespace DaresGameBot.Commands;

internal sealed class DrawQuestionCommand : CommandBase<Bot, Config>
{
    protected override string Name => "question";
    protected override string Description => Alias.ToLowerInvariant();

    protected override string Alias => Game.Game.DrawQuestionCaption;

    public DrawQuestionCommand(Bot bot) : base(bot) { }

    public override Task ExecuteAsync(Message message, bool fromChat, string? payload)
    {
        int replyToMessageId = fromChat ? message.MessageId : 0;
        return Manager.DrawAsync(Bot, message.Chat.Id, replyToMessageId, false);
    }
}
