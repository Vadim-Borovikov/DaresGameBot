namespace DaresGameBot.Operations.Data.PlayerListUpdates;

internal sealed class TogglePlayerData : PlayerListUpdateData
{
    public TogglePlayerData(string name, byte? order = null) : base(name, order) { }
}