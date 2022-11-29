using System.Threading.Tasks;
using DaresGameBot.Game;
using Telegram.Bot.Types;

namespace DaresGameBot.Commands;

internal sealed class DrawQuestionCommand : CommandWithAlias
{
    protected override string Alias => Game.Game.DrawQuestionCaption;

    public DrawQuestionCommand(Bot bot) : base(bot, "question", Game.Game.DrawQuestionCaption.ToLowerInvariant()) { }

    public override Task ExecuteAsync(Message message, Chat chat, string? payload)
    {
        int replyToMessageId = AbstractBot.Utils.IsGroup(chat) ? message.MessageId : 0;
        return Manager.DrawAsync(Bot, chat, replyToMessageId, false);
    }
}