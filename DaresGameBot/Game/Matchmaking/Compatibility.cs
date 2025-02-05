using DaresGameBot.Game.Players;

namespace DaresGameBot.Game.Matchmaking;

internal static class Compatibility
{
    public static bool AreCompatable(PlayerInfo p1, PlayerInfo p2)
    {
        if (!p1.Active || !p2.Active)
        {
            return false;
        }

        GroupChecker checker1 = p1.GroupChecker;
        GroupChecker checker2 = p2.GroupChecker;

        return checker1.WouldInteractWith(checker2) && checker2.WouldInteractWith(checker1);
    }
}