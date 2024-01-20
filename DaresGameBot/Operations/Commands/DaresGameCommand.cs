using System.Threading.Tasks;
using AbstractBot.Extensions;
using AbstractBot.Operations.Commands;
using AbstractBot.Operations.Data;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DaresGameBot.Operations.Commands;

internal abstract class DaresGameCommand : CommandSimple
{
    protected abstract string? Alias { get; }

    protected DaresGameCommand(Bot bot, string command, string description) : base(bot, command, description) { }

    protected override bool IsInvokingBy(Message message, User sender, out CommandDataSimple? data)
    {
        return base.IsInvokingBy(message, sender, out data)
               || ((message.Chat.Type == ChatType.Private) && (message.Text == Alias));
    }

    protected override Task ExecuteAsync(Message message, User sender)
    {
        int replyToMessageId = message.Chat.IsGroup() ? message.MessageId : 0;
        return ExecuteAsync(message.Chat, replyToMessageId);
    }

    protected abstract Task ExecuteAsync(Chat chat, int replyToMessageId);
}