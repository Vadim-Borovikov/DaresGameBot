using DaresGameBot.Game.States.Data;
using GryphonUtilities.Save;

namespace DaresGameBot.Game.States;

internal sealed class PlayerInfo : IStateful<PlayerData>
{
    public string? Username;
    public GroupsInfo GroupInfo;
    public bool Active;

    public PlayerInfo(PlayerData data)
    {
        Username = data.Username;
        GroupInfo = new GroupsInfo(data.GroupsData.Group, data.GroupsData.CompatableGroups);
        Active = data.Active;
    }

    public PlayerInfo(string? username, GroupsInfo groupInfo, bool active = true)
    {
        if (!string.IsNullOrWhiteSpace(username))
        {
            Username = username;
        }
        GroupInfo = groupInfo;
        Active = active;
    }

    public PlayerData Save()
    {
        return new PlayerData
        {
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

        Username = data.Username;
        GroupInfo = new GroupsInfo(data.GroupsData.Group, data.GroupsData.CompatableGroups);
        Active = data.Active;
    }
}