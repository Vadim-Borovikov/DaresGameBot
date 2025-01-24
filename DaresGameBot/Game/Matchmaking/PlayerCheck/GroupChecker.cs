using System.Collections.Generic;

namespace DaresGameBot.Game.Matchmaking.PlayerCheck;

internal sealed class GroupChecker : IPartnerChecker
{
    public readonly string Group;
    public readonly HashSet<string> CompatableGroups;

    public GroupChecker(string group, HashSet<string> compatableGroups)
    {
        Group = group;
        CompatableGroups = compatableGroups;
    }

    public bool WouldInteractWith(IPartnerChecker other)
    {
        return other is GroupChecker o && WouldInteractWith(o);
    }

    private bool WouldInteractWith(GroupChecker other) => CompatableGroups.Contains(other.Group);
}