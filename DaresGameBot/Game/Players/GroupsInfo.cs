using System;
using System.Collections.Generic;

namespace DaresGameBot.Game.Players;

internal readonly struct GroupsInfo : IEquatable<GroupsInfo>
{
    public readonly string Group;
    public readonly HashSet<string> CompatableGroups;

    public GroupsInfo(string group, HashSet<string> compatableGroups)
    {
        Group = group;
        CompatableGroups = compatableGroups;
    }

    public bool Equals(GroupsInfo other)
    {
        return (Group == other.Group) && CompatableGroups.SetEquals(other.CompatableGroups);
    }

    public static bool operator ==(GroupsInfo left, GroupsInfo right) => left.Equals(right);
    public static bool operator !=(GroupsInfo left, GroupsInfo right) => !left.Equals(right);

    public override bool Equals(object? obj) => obj is GroupsInfo other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Group, CompatableGroups);
}