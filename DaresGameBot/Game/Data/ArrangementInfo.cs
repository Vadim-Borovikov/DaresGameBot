using System.Collections.Generic;

namespace DaresGameBot.Game.Data;

internal sealed class ArrangementInfo
{
    public readonly int Hash;
    public readonly IReadOnlyList<string> Partners;
    public readonly IReadOnlyList<string> Helpers;

    public ArrangementInfo(int hash, IReadOnlyList<string> partners, IReadOnlyList<string> helpers)
    {
        Hash = hash;
        Partners = partners;
        Helpers = helpers;
    }
}