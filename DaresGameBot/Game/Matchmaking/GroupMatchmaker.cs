using System.Collections.Generic;
using DaresGameBot.Game.Data;

namespace DaresGameBot.Game.Matchmaking;

internal sealed class GroupMatchmaker : Matchmaker
{
    public GroupMatchmaker(Dictionary<string, GroupMatchmakerPlayerInfo> infos) => _infos = infos;

    public override bool AreCompatible(Player p1, Player p2)
    {
        if (!base.AreCompatible(p1, p2))
        {
            return false;
        }

        GroupMatchmakerPlayerInfo info1 = _infos[p1.Name];
        GroupMatchmakerPlayerInfo info2 = _infos[p2.Name];

        return info1.CompatableGroups.Contains(info2.Group) && info2.CompatableGroups.Contains(info1.Group);
    }

    private readonly Dictionary<string, GroupMatchmakerPlayerInfo> _infos;
}