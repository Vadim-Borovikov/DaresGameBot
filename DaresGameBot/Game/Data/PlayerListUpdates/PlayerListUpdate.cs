namespace DaresGameBot.Game.Data.PlayerListUpdates;

internal abstract class PlayerListUpdate
{
    public readonly string Name;

    protected PlayerListUpdate(string name) => Name = name;
}