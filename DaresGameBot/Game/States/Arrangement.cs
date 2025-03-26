using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Game.Data;
using DaresGameBot.Game.States.Data;
using GryphonUtilities.Save;

namespace DaresGameBot.Game.States;

internal sealed class Arrangement : IStateful<ArrangementData>
{
    public readonly List<string> Partners;
    public bool CompatablePartners;

    public Arrangement() => Partners = new List<string>();

    public Arrangement(List<string> partners, bool compatablePartners)
    {
        Partners = partners;
        CompatablePartners = compatablePartners;
    }

    public ArrangementType GetArrangementType() => new((byte)Partners.Count, CompatablePartners);

    public ArrangementData Save()
    {
        return new ArrangementData
        {
            Partners = Partners.ToList(),
            CompatablePartners = CompatablePartners
        };
    }

    public void LoadFrom(ArrangementData? data)
    {
        if (data is null)
        {
            return;
        }

        Partners.Clear();
        Partners.AddRange(data.Partners);

        CompatablePartners = data.CompatablePartners;
    }
}