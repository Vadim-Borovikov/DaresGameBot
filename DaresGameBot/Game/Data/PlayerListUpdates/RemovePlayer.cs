namespace DaresGameBot.Game.Data.PlayerListUpdates;

internal sealed class RemovePlayer : PlayerListUpdate
{
    public RemovePlayer(string name) : base(name) { }
}