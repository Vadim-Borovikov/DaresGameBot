using System;

namespace DaresGameBot.Game.Players;

internal sealed class PlayerInfo
{
    public GroupsInfo GroupInfo;
    public ushort Points;
    public bool Active;

    public PlayerInfo(GroupsInfo groupInfo, ushort points)
    {
        GroupInfo = groupInfo;
        Points = points;
        Active = true;
    }

    public void ActivateWith(ushort points)
    {
        Active = true;
        Points = Math.Max(Points, points);
    }
}