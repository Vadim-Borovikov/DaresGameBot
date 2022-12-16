using System.Threading.Tasks;
using DaresGameBot.Game;
using Telegram.Bot.Types;

namespace DaresGameBot.Commands;

internal sealed class DrawQuestionCommand : DaresGameCommand
{
    protected override int Priority => 4;

    protected override string Alias => Game.Game.DrawQuestionCaption;

    public DrawQuestionCommand(Bot bot) : base(bot, "question", Game.Game.DrawQuestionCaption.ToLowerInvariant()) { }

    protected override Task ExecuteAsync(Chat chat, int replyToMessageId)
    {
        return Manager.DrawAsync(Bot, chat, replyToMessageId, false);
    }
}