using System.Threading.Tasks;
using AbstractBot.Commands;
using DaresGameBot.Game;
using Telegram.Bot.Types;

namespace DaresGameBot.Commands;

internal sealed class DrawQuestionCommand : CommandBaseCustom<Bot, Config>
{
    protected override string Alias => Game.Game.DrawQuestionCaption;

    public DrawQuestionCommand(Bot bot) : base(bot, "question", Game.Game.DrawQuestionCaption.ToLowerInvariant()) { }

    public override Task ExecuteAsync(Message message, bool fromChat, string? payload)
    {
        int replyToMessageId = fromChat ? message.MessageId : 0;
        return Manager.DrawAsync(Bot, message.Chat, replyToMessageId, false);
    }
}