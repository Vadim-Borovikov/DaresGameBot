using JetBrains.Annotations;
using System.Collections.Generic;

namespace DaresGameBot.Game.States.Data;

public sealed class ArrangementData
{
    [UsedImplicitly]
    public List<long> Partners { get; set; } = null!;

    [UsedImplicitly]
    public bool CompatablePartners { get; set; }
}