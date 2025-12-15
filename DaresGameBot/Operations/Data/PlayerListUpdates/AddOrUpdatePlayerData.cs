using DaresGameBot.Game.States;

namespace DaresGameBot.Operations.Data.PlayerListUpdates;

internal sealed class AddOrUpdatePlayerData
{
    public readonly string Name;
    public readonly GroupsInfo Info;

    public AddOrUpdatePlayerData(string name, GroupsInfo info)
    {
        Name = name;
        Info = info;
    }
}