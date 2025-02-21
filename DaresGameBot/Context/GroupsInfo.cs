using System;
using System.Collections.Generic;
using System.Linq;
using AbstractBot;
using DaresGameBot.Save;

namespace DaresGameBot.Context;

internal sealed class GroupsInfo : IEquatable<GroupsInfo>, IContext<GroupsInfo, GroupsData, object>
{
    public readonly string Group;
    public readonly HashSet<string> CompatableGroups;

    public GroupsInfo(string group, HashSet<string> compatableGroups)
    {
        Group = group;
        CompatableGroups = compatableGroups;
    }

    public bool Equals(GroupsInfo? other)
    {
        return (Group == other?.Group) && CompatableGroups.SetEquals(other.CompatableGroups);
    }

    public static bool operator ==(GroupsInfo left, GroupsInfo right) => left.Equals(right);
    public static bool operator !=(GroupsInfo left, GroupsInfo right) => !left.Equals(right);

    public override bool Equals(object? obj) => obj is GroupsInfo other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Group, CompatableGroups);

    public GroupsData Save()
    {
        return new GroupsData
        {
            Group = Group,
            CompatableGroups = CompatableGroups.ToList()
        };
    }

    public static GroupsInfo Load(GroupsData data, object? meta)
    {
        HashSet<string> compatableGroups = new(data.CompatableGroups);
        return new GroupsInfo(data.Group, compatableGroups);
    }
}