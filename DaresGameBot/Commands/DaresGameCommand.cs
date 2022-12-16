using AbstractBot.Operations;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DaresGameBot.Commands;

internal abstract class DaresGameCommand : CommandOperation
{
    protected abstract string? Alias { get; }

    protected DaresGameCommand(Bot bot, string command, string description) : base(bot, command, description)
    {
        Bot = bot;
    }

    protected override bool IsInvokingBy(Message message, Chat sender, out string? payload)
    {
        return base.IsInvokingBy(message, sender, out payload)
               || ((message.Chat.Type == ChatType.Private) && (message.Text == Alias));
    }

    protected override Task ExecuteAsync(Message message, Chat sender, string? _)
    {
        Chat chat = BotBase.GetReplyChatFor(message, sender);
        int replyToMessageId = AbstractBot.Utils.IsGroup(chat) ? message.MessageId : 0;
        return ExecuteAsync(chat, replyToMessageId);
    }

    protected abstract Task ExecuteAsync(Chat chat, int replyToMessageId);

    protected readonly Bot Bot;
}