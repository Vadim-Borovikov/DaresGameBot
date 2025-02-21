using AbstractBot;
using DaresGameBot.Save;

namespace DaresGameBot.Context;

internal sealed class PlayerInfo : IContext<PlayerInfo, PlayerData, object>
{
    public GroupsInfo GroupInfo;
    public bool Active;

    public PlayerInfo(GroupsInfo groupInfo, bool active = true)
    {
        GroupInfo = groupInfo;
        Active = active;
    }

    public PlayerData Save()
    {
        return new PlayerData
        {
            GroupInfo = GroupInfo.Save(),
            Active = Active
        };
    }

    public static PlayerInfo Load(PlayerData data, object? meta)
    {
        return new PlayerInfo(GroupsInfo.Load(data.GroupInfo, meta), data.Active);
    }
}