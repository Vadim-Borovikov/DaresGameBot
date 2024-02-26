namespace DaresGameBot.Game.Data;

internal sealed class Player
{
    public readonly string Name;

    public Player(string name) => Name = name;

    /*public static Player? From(string s)
    {
        string[] parts = s.Split(PartsSeparator);
        return parts.Length != 3 ? null : new Player(parts[0], parts[1], parts[2].Split(GroupsSeparator));
    }

    private Player(string name, string group, IEnumerable<string> compatableGroups)
    {
        Name = name;

        _group = group;
        _compatableGroups = new HashSet<string>(compatableGroups);
    }*/

    /*private const string PartsSeparator = ",";
    private const string GroupsSeparator = "+";*/
}