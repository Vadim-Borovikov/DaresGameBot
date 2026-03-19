using DaresGameBot.Game.States;

namespace DaresGameBot.Operations.Data.PlayerListUpdates;

internal sealed class AddOrUpdatePlayerData
{
    public readonly string Name;
    public readonly string? Username;
    public readonly GroupsInfo Info;

    public AddOrUpdatePlayerData(string name, string? username, GroupsInfo info)
    {
        Name = name;
        Username = username;
        Info = info;
    }
}