using DaresGameBot.Game.States;

namespace DaresGameBot.Operations.Data.PlayerListUpdates;

internal sealed class AddOrUpdatePlayerData : PlayerListUpdateData
{
    public readonly GroupsInfo Info;

    public AddOrUpdatePlayerData(string name, GroupsInfo info, byte? index = null) : base(name, index) => Info = info;
}