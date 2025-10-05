using System;
using System.Threading.Tasks;
using AbstractBot.Models.Operations;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations;

internal sealed class ToggleInactivePlayers : Operation
{
    public override Enum AccessRequired => Bot.AccessType.Admin;

    public ToggleInactivePlayers(Bot bot) : base(bot.Core.Accesses, bot.Core.UpdateSender) => _bot = bot;

    protected override bool IsInvokingBy(Message message, User sender, string callbackQueryDataCore) => true;

    protected override Task ExecuteAsync(Message message, User sender, string callbackQueryDataCore)
    {
        return _bot.ToggleInactivePlayersVisibility();
    }

    private readonly Bot _bot;
}