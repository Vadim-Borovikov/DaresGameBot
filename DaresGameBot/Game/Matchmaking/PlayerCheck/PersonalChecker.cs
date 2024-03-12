using System.Collections.Generic;

namespace DaresGameBot.Game.Matchmaking.PlayerCheck;

internal sealed class PersonalChecker : IPartnerChecker
{
    public readonly long Id;
    public readonly HashSet<long> CompatablePlayerIds;

    public PersonalChecker(long id, HashSet<long> compatablePlayerIds)
    {
        Id = id;
        CompatablePlayerIds = compatablePlayerIds;
    }

    public bool WouldInteractWith(IPartnerChecker other)
    {
        return other is PersonalChecker o && WouldInteractWith(o);
    }

    private bool WouldInteractWith(PersonalChecker other) => CompatablePlayerIds.Contains(other.Id);
}