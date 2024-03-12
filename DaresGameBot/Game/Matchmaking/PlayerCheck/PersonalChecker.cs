using System.Collections.Generic;

namespace DaresGameBot.Game.Matchmaking.PlayerCheck;

internal sealed class PersonalChecker : IPartnerChecker
{
    public readonly long Id;
    public readonly HashSet<long> CompatablePlayerIds = new();

    public PersonalChecker(long id) => Id = id;

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