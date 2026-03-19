using AbstractBot.Models.Operations;
using System;
using System.Threading.Tasks;
using DaresGameBot.Game.States;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations;

internal sealed class AcceptPlayerName : Operation<string>
{
    public override Enum AccessRequired => Bot.AccessType.Player;

    public AcceptPlayerName(Bot bot, BotState state) : base(bot.Core.Accesses, bot.Core.UpdateSender)
    {
        _bot = bot;
        _state = state;
    }

    protected override bool IsInvokingBy(Message message, User? sender, out string name)
    {
        if (string.IsNullOrEmpty(message.Text))
        {
            name = string.Empty;
            return false;
        }

        name = message.Text;

        return sender is not null && _state.UserStates.ContainsKey(sender.Id)
                                  && (_state.UserStates[sender.Id].State == UserState.StateType.EnteringName);
    }

    protected override Task ExecuteAsync(string name, Message message, User sender)
    {
        return _bot.AcceptPlayerNameAsync(message.Chat, sender, name);
    }

    private readonly Bot _bot;
    private readonly BotState _state;
}