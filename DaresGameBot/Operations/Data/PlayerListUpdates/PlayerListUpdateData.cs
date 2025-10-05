namespace DaresGameBot.Operations.Data.PlayerListUpdates;

internal abstract class PlayerListUpdateData
{
    public readonly string Name;
    public readonly byte? Index;

    protected PlayerListUpdateData(string name, byte? index)
    {
        Name = name;
        Index = index;
    }
}