using DaresGameBot.Game.States;

namespace DaresGameBot.Operations.Data.PlayerListUpdates;

internal sealed class AddOrUpdatePlayerData
{
    public readonly long Id;
    public readonly string Name;
    public readonly GroupsInfo Info;

    public AddOrUpdatePlayerData(long id, string name, GroupsInfo info)
    {
        Id = id;
        Name = name;
        Info = info;
    }
}