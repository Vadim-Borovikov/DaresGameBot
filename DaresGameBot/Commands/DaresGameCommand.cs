using AbstractBot.Operations;
using System.Threading.Tasks;
using AbstractBot.Extensions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DaresGameBot.Commands;

internal abstract class DaresGameCommand : CommandOperation
{
    protected abstract string? Alias { get; }

    protected DaresGameCommand(Bot bot, string command, string description) : base(bot, command, description)
    {
        GameManager = bot.GameManager;
    }

    protected override bool IsInvokingBy(Message message, out string? payload)
    {
        return base.IsInvokingBy(message, out payload)
               || ((message.Chat.Type == ChatType.Private) && (message.Text == Alias));
    }

    protected override Task ExecuteAsync(Message message, long senderId, string? _)
    {
        int replyToMessageId = message.Chat.IsGroup() ? message.MessageId : 0;
        return ExecuteAsync(message.Chat, replyToMessageId);
    }

    protected abstract Task ExecuteAsync(Chat chat, int replyToMessageId);

    protected readonly Game.Manager GameManager;
}