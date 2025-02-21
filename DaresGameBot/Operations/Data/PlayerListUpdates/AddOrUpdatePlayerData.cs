using DaresGameBot.Context;

namespace DaresGameBot.Operations.Data.PlayerListUpdates;

internal sealed class AddOrUpdatePlayerData : PlayerListUpdateData
{
    public readonly GroupsInfo Info;

    public AddOrUpdatePlayerData(string name, GroupsInfo info) : base(name) => Info = info;
}