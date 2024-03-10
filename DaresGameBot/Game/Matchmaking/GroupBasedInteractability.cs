using System.Collections.Generic;

namespace DaresGameBot.Game.Matchmaking;

internal sealed class GroupBasedInteractability : IInteractabilityProvider
{
    public GroupBasedInteractability(string group, HashSet<string> compatableGroups)
    {
        _group = group;
        _compatableGroups = compatableGroups;
    }

    public bool WouldInteractWith(IInteractabilityProvider other)
    {
        return other is GroupBasedInteractability o && WouldInteractWith(o);
    }

    private bool WouldInteractWith(GroupBasedInteractability other) => _compatableGroups.Contains(other._group);

    private readonly string _group;
    private readonly HashSet<string> _compatableGroups;
}