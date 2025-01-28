using System.Collections.Generic;
using DaresGameBot.Game.Data;

namespace DaresGameBot.Game;

internal sealed class Arrangement
{
    public readonly IReadOnlyList<string> Partners;
    public readonly bool CompatablePartners;

    public Arrangement(IReadOnlyList<string> partners, bool compatablePartners)
    {
        Partners = partners;
        CompatablePartners = compatablePartners;
    }

    public ArrangementType GetArrangementType() => new((byte)Partners.Count, CompatablePartners);
}