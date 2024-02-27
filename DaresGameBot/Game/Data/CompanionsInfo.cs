using System.Collections.Generic;

namespace DaresGameBot.Game.Data;

internal sealed class CompanionsInfo
{
    public readonly Player Player;
    public readonly IReadOnlyList<Player>? Partners;
    public readonly IReadOnlyList<Player>? Helpers;

    public CompanionsInfo(Player player, IReadOnlyList<Player>? partners, IReadOnlyList<Player>? helpers)
    {
        Player = player;
        Partners = partners;
        Helpers = helpers;
    }
}