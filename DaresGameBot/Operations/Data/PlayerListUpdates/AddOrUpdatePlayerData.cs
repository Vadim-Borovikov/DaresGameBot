using DaresGameBot.Game.States;

namespace DaresGameBot.Operations.Data.PlayerListUpdates;

internal sealed class AddOrUpdatePlayerData
{
    public readonly string Name;
    public readonly string? Handler;
    public readonly GroupsInfo Info;

    public AddOrUpdatePlayerData(string name, string? handler, GroupsInfo info)
    {
        Name = name;
        Handler = handler;
        Info = info;
    }
}