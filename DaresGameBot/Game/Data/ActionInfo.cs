using System.Collections.Generic;

namespace DaresGameBot.Game.Data;

internal sealed class ActionInfo
{
    public readonly string Player;
    public readonly IReadOnlyList<string> Partners;
    public readonly ushort ActionId;

    public ActionInfo(string player, IReadOnlyList<string> partners, ushort actionId)
    {
        Player = player;
        Partners = partners;
        ActionId = actionId;
    }
}