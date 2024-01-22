using System.Collections.Generic;

namespace DaresGameBot.Game.Data;

// ReSharper disable NullableWarningSuppressionIsUsed

internal sealed class Player
{
    public readonly string Name;

    public static Player? From(string s)
    {
        string[] parts = s.Split(PartsSeparator);
        return parts.Length != 3 ? null : new Player(parts[0], parts[1], parts[2].Split(GroupsSeparator));
    }

    private Player(string name, string group, IEnumerable<string> compatableGroups)
    {
        Name = name;

        _group = group;
        _compatableGroups = new HashSet<string>(compatableGroups);
    }

    public static bool AreCompatable(IReadOnlyList<Player> players)
    {
        for (int i = 0; i < players.Count; i++)
        {
            for (int j = i + 1; j < players.Count; j++)
            {
                if (!players[i].IsCompatableWith(players[j]))
                {
                    return false;
                }
            }
        }
        return true;
    }

    public bool IsCompatableWith(Player other)
    {
        return (other != this) && _compatableGroups.Contains(other._group) && other._compatableGroups.Contains(_group);
    }

    private readonly string _group;
    private readonly HashSet<string> _compatableGroups;

    private const string PartsSeparator = ",";
    private const string GroupsSeparator = "+";
}