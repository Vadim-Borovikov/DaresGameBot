using DaresGameBot.Game.States;

namespace DaresGameBot.Game;

internal sealed class ActionInfo
{
    public readonly ushort Id;
    public readonly Arrangement Arrangement;

    public ActionInfo(ushort id, Arrangement arrangement)
    {
        Id = id;
        Arrangement = arrangement;
    }
}