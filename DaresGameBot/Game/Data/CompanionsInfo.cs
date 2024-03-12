using System.Collections.Generic;

namespace DaresGameBot.Game.Data;

internal sealed class CompanionsInfo
{
    public readonly string Player;
    public readonly IReadOnlyList<string>? Partners;
    public readonly IReadOnlyList<string>? Helpers;

    public CompanionsInfo(string player, IReadOnlyList<string>? partners, IReadOnlyList<string>? helpers)
    {
        Player = player;
        Partners = partners;
        Helpers = helpers;
    }
}