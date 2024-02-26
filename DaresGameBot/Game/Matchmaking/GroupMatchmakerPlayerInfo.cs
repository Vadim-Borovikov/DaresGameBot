using System.Collections.Generic;

namespace DaresGameBot.Game.Matchmaking;

internal sealed class GroupMatchmakerPlayerInfo
{
    public readonly string Group;
    public readonly HashSet<string> CompatableGroups;

    public GroupMatchmakerPlayerInfo(string group, HashSet<string> compatableGroups)
    {
        Group = group;
        CompatableGroups = compatableGroups;
    }
}