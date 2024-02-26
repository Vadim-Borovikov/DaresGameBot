using System.Collections.Generic;
using DaresGameBot.Game.Data;

namespace DaresGameBot.Game.Matchmaking;

internal sealed class GroupBasedCompatibility : Compatibility
{
    public GroupBasedCompatibility(Dictionary<string, GroupBasedCompatibilityPlayerInfo> infos) => _infos = infos;

    public override bool Check(Player p1, Player p2)
    {
        if (!base.Check(p1, p2))
        {
            return false;
        }

        GroupBasedCompatibilityPlayerInfo info1 = _infos[p1.Name];
        GroupBasedCompatibilityPlayerInfo info2 = _infos[p2.Name];

        return info1.CompatableGroups.Contains(info2.Group) && info2.CompatableGroups.Contains(info1.Group);
    }

    private readonly Dictionary<string, GroupBasedCompatibilityPlayerInfo> _infos;
}