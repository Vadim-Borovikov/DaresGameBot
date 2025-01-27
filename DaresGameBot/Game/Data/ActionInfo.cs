namespace DaresGameBot.Game.Data;

internal sealed class ActionInfo
{
    public readonly ArrangementInfo ArrangementInfo;
    public readonly ushort ActionId;

    public ActionInfo(ArrangementInfo arrangementInfo, ushort actionId)
    {
        ArrangementInfo = arrangementInfo;
        ActionId = actionId;
    }
}