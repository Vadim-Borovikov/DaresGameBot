using System.Collections.Generic;

namespace DaresGameBot.Game.Data;

internal sealed class ArrangementInfo
{
    public readonly int Hash;
    public readonly string Player;
    public readonly IReadOnlyList<string> Partners;
    public readonly IReadOnlyList<string> Helpers;

    public ArrangementInfo(int hash, string player, IReadOnlyList<string> partners, IReadOnlyList<string> helpers)
    {
        Hash = hash;
        Player = player;
        Partners = partners;
        Helpers = helpers;
    }
}