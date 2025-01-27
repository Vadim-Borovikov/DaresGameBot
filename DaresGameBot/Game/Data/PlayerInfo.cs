using System.Collections.Generic;
using DaresGameBot.Game.Matchmaking.PlayerCheck;

namespace DaresGameBot.Game.Data;

internal sealed class PlayerInfo
{
    public GroupChecker GroupChecker;
    public ushort Points;
    public bool Active;
    public HashSet<int> PlayableArrangements = new();

    public PlayerInfo(GroupChecker groupChecker, ushort points)
    {
        GroupChecker = groupChecker;
        Points = points;
        Active = true;
    }
}