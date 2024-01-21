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

    public override string ToString() => Name;

    public static bool AreCompatable(Player p1, Player p2)
    {
        return (p1 != p2) && p1._compatableGroups.Contains(p2._group) && p2._compatableGroups.Contains(p1._group);
    }

    private readonly string _group;
    private readonly HashSet<string> _compatableGroups;

    private const string PartsSeparator = ";";
    private const string GroupsSeparator = ",";
}