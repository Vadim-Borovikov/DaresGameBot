using System.Collections.Generic;

namespace DaresGameBot.Game.Matchmaking.PlayerCheck;

internal sealed class GroupChecker : IPartnerChecker
{
    public GroupChecker(string group, HashSet<string> compatableGroups)
    {
        _group = group;
        _compatableGroups = compatableGroups;
    }

    public bool WouldInteractWith(IPartnerChecker other)
    {
        return other is GroupChecker o && WouldInteractWith(o);
    }

    private bool WouldInteractWith(GroupChecker other) => _compatableGroups.Contains(other._group);

    private readonly string _group;
    private readonly HashSet<string> _compatableGroups;
}