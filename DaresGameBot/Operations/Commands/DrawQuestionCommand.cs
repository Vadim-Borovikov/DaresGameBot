using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations.Commands;

internal sealed class DrawQuestionCommand : DaresGameCommand
{
    protected override byte Order => 4;

    protected override string Alias => Game.Game.DrawQuestionCaption;

    public DrawQuestionCommand(Bot bot) : base(bot, "question", Game.Game.DrawQuestionCaption.ToLowerInvariant()) { }

    protected override Task ExecuteAsync(Chat chat, int replyToMessageId)
    {
        return GameManager.DrawAsync(chat, replyToMessageId, false);
    }
}