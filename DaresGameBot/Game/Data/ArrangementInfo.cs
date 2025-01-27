using System.Collections.Generic;

namespace DaresGameBot.Game.Data;

internal sealed class ArrangementInfo
{
    public readonly int Hash;
    public readonly IReadOnlyList<string> Partners;

    public ArrangementInfo(int hash, IReadOnlyList<string> partners)
    {
        Hash = hash;
        Partners = partners;
    }
}