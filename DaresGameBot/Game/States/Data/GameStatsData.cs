using System.Collections.Generic;
using JetBrains.Annotations;

namespace DaresGameBot.Game.States.Data;

public sealed class GameStatsData
{
    [UsedImplicitly]
    public Dictionary<long, uint> Points { get; set; } = null!;
    [UsedImplicitly]
    public Dictionary<string, uint> Propositions { get; set; } = null!;
    [UsedImplicitly]
    public Dictionary<long, uint> PartnerPropositions { get; set; } = null!;
    [UsedImplicitly]
    public Dictionary<long, uint> Turns { get; set; } = null!;
    [UsedImplicitly]
    public uint CurrentRound { get; set; }
    [UsedImplicitly]
    public uint? MinRound { get; set; }
}