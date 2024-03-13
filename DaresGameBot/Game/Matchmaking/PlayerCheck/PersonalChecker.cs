using System.Collections.Generic;
using Telegram.Bot.Types;

namespace DaresGameBot.Game.Matchmaking.PlayerCheck;

internal sealed class PersonalChecker : IPartnerChecker
{
    public long Id => Chat.Id;

    public readonly HashSet<long> CompatablePlayerIds = new();

    public readonly Chat Chat;
    public Message? PreferencesMessage;

    public PersonalChecker(Chat chat) => Chat = chat;

    public bool WouldInteractWith(IPartnerChecker other)
    {
        return other is PersonalChecker o && WouldInteractWith(o);
    }

    public void Toggle(long id)
    {
        if (CompatablePlayerIds.Contains(id))
        {
            CompatablePlayerIds.Remove(id);
        }
        else
        {
            CompatablePlayerIds.Add(id);
        }
    }

    private bool WouldInteractWith(PersonalChecker other) => CompatablePlayerIds.Contains(other.Id);
}