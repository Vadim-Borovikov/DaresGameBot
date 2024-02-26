using System.Collections.Generic;

namespace DaresGameBot.Game.Matchmaking;

internal sealed class GroupBasedCompatibilityPlayerInfo
{
    public readonly string Group;
    public readonly HashSet<string> CompatableGroups;

    public GroupBasedCompatibilityPlayerInfo(string group, HashSet<string> compatableGroups)
    {
        Group = group;
        CompatableGroups = compatableGroups;
    }
}