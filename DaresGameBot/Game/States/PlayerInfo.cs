using DaresGameBot.Game.States.Data;
using GryphonUtilities.Save;

namespace DaresGameBot.Game.States;

internal sealed class PlayerInfo : IStateful<PlayerData>
{
    public string Name;
    public GroupsInfo GroupInfo;
    public bool Active;

    public PlayerInfo(string name, GroupsInfo groupInfo, bool active = true)
    {
        Name = name;
        GroupInfo = groupInfo;
        Active = active;
    }

    public PlayerData Save()
    {
        return new PlayerData
        {
            Name = Name,
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

        Name = data.Name;
        GroupInfo = new GroupsInfo(data.GroupsData.Group, data.GroupsData.CompatableGroups);
        Active = data.Active;
    }
}