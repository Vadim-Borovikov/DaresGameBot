using AbstractBot.Commands;

namespace DaresGameBot.Commands;

internal abstract class CommandWithAlias : CommandBaseCustom<Bot, Config>
{
    protected abstract string? Alias { get; }

    protected CommandWithAlias(Bot bot, string command, string description) : base(bot, command, description) { }

    public override bool IsInvokingBy(string? text, bool fromChat, string? botName, out string? payload)
    {
        return base.IsInvokingBy(text, fromChat, botName, out payload)
               || (!string.IsNullOrWhiteSpace(Alias) && (text == Alias));
    }
}