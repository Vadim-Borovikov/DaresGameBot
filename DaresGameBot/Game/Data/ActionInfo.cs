using System.Collections.Generic;

namespace DaresGameBot.Game.Data;

internal sealed class ActionInfo
{
    public readonly ArrangementInfo ArrangementInfo;
    public readonly ushort ActionId;
    public readonly IReadOnlyList<string> Helpers;

    public ActionInfo(ArrangementInfo arrangementInfo, ushort actionId, IReadOnlyList<string> helpers)
    {
        ArrangementInfo = arrangementInfo;
        ActionId = actionId;
        Helpers = helpers;
    }
}