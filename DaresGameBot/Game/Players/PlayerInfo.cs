namespace DaresGameBot.Game.Players;

internal sealed class PlayerInfo
{
    public GroupsInfo GroupInfo;
    public bool Active;

    public PlayerInfo(GroupsInfo groupInfo)
    {
        GroupInfo = groupInfo;
        Active = true;
    }
}