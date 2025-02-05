using DaresGameBot.Game.Players;

namespace DaresGameBot.Game.Matchmaking.Compatibility;

internal sealed class GroupCompatibility : ICompatibility
{
    public bool AreCompatable(PlayerInfo p1, PlayerInfo p2)
    {
        if (!p1.Active || !p2.Active)
        {
            return false;
        }

        GroupsInfo g1 = p1.GroupInfo;
        GroupsInfo g2 = p2.GroupInfo;

        return g1.CompatableGroups.Contains(g2.Group) && g2.CompatableGroups.Contains(g1.Group);
    }
}