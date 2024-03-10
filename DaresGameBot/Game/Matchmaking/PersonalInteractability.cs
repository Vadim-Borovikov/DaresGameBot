using System.Collections.Generic;

namespace DaresGameBot.Game.Matchmaking;

internal sealed class PersonalInteractability : IInteractabilityProvider
{
    public readonly long Id;
    public readonly HashSet<long> CompatablePlayerIds;

    public PersonalInteractability(long id, HashSet<long> compatablePlayerIds)
    {
        Id = id;
        CompatablePlayerIds = compatablePlayerIds;
    }

    public bool WouldInteractWith(IInteractabilityProvider other)
    {
        return other is PersonalInteractability o && WouldInteractWith(o);
    }

    private bool WouldInteractWith(PersonalInteractability other) => CompatablePlayerIds.Contains(other.Id);
}