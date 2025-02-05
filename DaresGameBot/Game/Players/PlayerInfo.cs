using System;
using DaresGameBot.Game.Matchmaking;

namespace DaresGameBot.Game.Players;

internal sealed class PlayerInfo
{
    public GroupChecker GroupChecker;
    public ushort Points;
    public bool Active;

    public PlayerInfo(GroupChecker groupChecker, ushort points)
    {
        GroupChecker = groupChecker;
        Points = points;
        Active = true;
    }

    public void ActivateWith(ushort points)
    {
        Active = true;
        Points = Math.Max(Points, points);
    }
}