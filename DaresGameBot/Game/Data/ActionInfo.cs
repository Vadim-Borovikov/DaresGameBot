namespace DaresGameBot.Game.Data;

internal sealed class ActionInfo
{
    public readonly Arrangement Arrangement;
    public readonly ushort ActionId;

    public ActionInfo(Arrangement arrangement, ushort actionId)
    {
        Arrangement = arrangement;
        ActionId = actionId;
    }
}