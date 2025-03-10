using System;
using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Game.States.Data;

namespace DaresGameBot.Game.States;

internal sealed class GroupsInfo : IEquatable<GroupsInfo>
{
    public readonly string Group;
    public readonly HashSet<string> CompatableGroups;

    public GroupsInfo(string group, IEnumerable<string> compatableGroups)
        : this(group, new HashSet<string>(compatableGroups))
    {
    }

    public GroupsInfo(string group, HashSet<string> compatableGroups)
    {
        Group = group;
        CompatableGroups = compatableGroups;
    }

    public bool Equals(GroupsInfo? other)
    {
        return (Group == other?.Group) && CompatableGroups.SetEquals(other.CompatableGroups);
    }

    public GroupsData Save()
    {
        return new GroupsData
        {
            Group = Group,
            CompatableGroups = CompatableGroups.ToList()
        };
    }

    public static bool operator ==(GroupsInfo left, GroupsInfo right) => left.Equals(right);
    public static bool operator !=(GroupsInfo left, GroupsInfo right) => !left.Equals(right);

    public override bool Equals(object? obj) => obj is GroupsInfo other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Group, CompatableGroups);
}