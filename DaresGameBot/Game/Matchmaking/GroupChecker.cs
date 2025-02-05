using System;
using System.Collections.Generic;

namespace DaresGameBot.Game.Matchmaking;

internal readonly struct GroupChecker : IEquatable<GroupChecker>
{
    public GroupChecker(string group, HashSet<string> compatableGroups)
    {
        _group = group;
        _compatableGroups = compatableGroups;
    }

    public bool WouldInteractWith(GroupChecker other) => _compatableGroups.Contains(other._group);

    public bool Equals(GroupChecker other)
    {
        return (_group == other._group) && _compatableGroups.SetEquals(other._compatableGroups);
    }

    public static bool operator ==(GroupChecker left, GroupChecker right) => left.Equals(right);
    public static bool operator !=(GroupChecker left, GroupChecker right) => !left.Equals(right);

    public override bool Equals(object? obj) => obj is GroupChecker other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(_group, _compatableGroups);

    private readonly string _group;
    private readonly HashSet<string> _compatableGroups;
}