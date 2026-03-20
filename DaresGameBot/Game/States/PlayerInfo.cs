using DaresGameBot.Game.States.Data;
using GryphonUtilities.Save;
using System.Collections.Generic;

namespace DaresGameBot.Game.States;

internal sealed class PlayerInfo : IStateful<PlayerData>
{
    public string? Username;
    public HashSet<byte> Rounds;
    public GroupsInfo GroupInfo;
    public bool Active;

    public PlayerInfo(PlayerData data)
    {
        Username = data.Username;
        Rounds = new HashSet<byte>(data.Rounds);
        GroupInfo = new GroupsInfo(data.GroupsData.Group, data.GroupsData.CompatableGroups);
        Active = data.Active;
    }

    public PlayerInfo(string? username, HashSet<byte> rounds, GroupsInfo groupInfo, bool active = true)
    {
        if (!string.IsNullOrWhiteSpace(username))
        {
            Username = username;
        }
        Rounds = rounds;
        GroupInfo = groupInfo;
        Active = active;
    }

    public PlayerData Save()
    {
        return new PlayerData
        {
            Username = Username,
            Rounds = new List<byte>(Rounds),
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
        Rounds = new HashSet<byte>(data.Rounds);
        GroupInfo = new GroupsInfo(data.GroupsData.Group, data.GroupsData.CompatableGroups);
        Active = data.Active;
    }
}