using DaresGameBot.Game.States.Data;
using GryphonUtilities.Save;

namespace DaresGameBot.Game.States;

internal sealed class PlayerInfo : IStateful<PlayerData>
{
    public string Name;
    public string? Username;
    public GroupsInfo GroupInfo;
    public bool Active;

    public PlayerInfo()
    {
        Name = string.Empty;
        GroupInfo = new GroupsInfo();
        Active = false;
    }

    public PlayerInfo(string name, string? username, GroupsInfo groupInfo, bool active = true)
    {
        Name = name;
        Username = username;
        GroupInfo = groupInfo;
        Active = active;
    }

    public PlayerData Save()
    {
        return new PlayerData
        {
            Name = Name,
            Username = Username,
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
        Username = data.Username;
        GroupInfo = new GroupsInfo(data.GroupsData.Group, data.GroupsData.CompatableGroups);
        Active = data.Active;
    }
}