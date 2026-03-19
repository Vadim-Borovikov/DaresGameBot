using AbstractBot.Models.Operations;
using System;
using System.Threading.Tasks;
using DaresGameBot.Game.States;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations;

internal sealed class EditPlayerName : Operation
{
    public override Enum AccessRequired => Bot.AccessType.Player;

    public EditPlayerName(Bot bot, BotState state) : base(bot.Core.Accesses, bot.Core.UpdateSender)
    {
        _bot = bot;
        _state = state;
    }

    protected override bool IsInvokingBy(Message message, User? sender) => false;

    protected override bool IsInvokingBy(Message message, User? sender, string callbackQueryDataCore)
    {
        return sender is not null && _state.UserStates.ContainsKey(sender.Id);
    }

    protected override Task ExecuteAsync(Message message, User sender, string callbackQueryDataCore)
    {
        return _bot.EditPlayerNameAsync(message.Chat, sender);
    }

    private readonly Bot _bot;
    private readonly BotState _state;
}