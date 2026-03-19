using AbstractBot.Models.Operations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations;

internal sealed class TogglePartnersGender : Operation<string>
{
    public override Enum AccessRequired => Bot.AccessType.Player;

    public TogglePartnersGender(Bot bot, HashSet<string> genders) : base(bot.Core.Accesses, bot.Core.UpdateSender)
    {
        _bot = bot;
        _genders = genders;
    }

    protected override bool IsInvokingBy(Message message, User? sender, out string? gender)
    {
        gender = null;
        return false;
    }

    protected override bool IsInvokingBy(Message message, User? sender, string callbackQueryDataCore,
        out string? gender)
    {
        gender = callbackQueryDataCore;
        return _genders.Contains(gender);
    }

    protected override Task ExecuteAsync(string gender, Message message, User sender)
    {
        return Task.CompletedTask; //return _bot.TogglePartnersGender(message.Chat, gender, sender);
    }

    private readonly Bot _bot;
    private readonly HashSet<string> _genders;
}