using DaresGameBot.Game.States.Data;
using GryphonUtilities.Save;

namespace DaresGameBot.Game.States;

internal sealed class PlayerInfo : IStateful<PlayerData>
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
            GroupsData = GroupInfo.Save(),
            Active = Active
        };
    }

    public void LoadFrom(PlayerData? data)
    {
        if (data is null)
        {
            return;
        }

        GroupInfo = new GroupsInfo(data.GroupsData.Group, data.GroupsData.CompatableGroups);
        Active = data.Active;
    }
}