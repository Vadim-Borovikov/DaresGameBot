namespace DaresGameBot.Operations.Data.PlayerListUpdates;

internal abstract class PlayerListUpdateData
{
    public readonly string Name;

    protected PlayerListUpdateData(string name) => Name = name;
}