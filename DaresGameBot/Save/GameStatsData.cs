﻿using System.Collections.Generic;
using JetBrains.Annotations;

namespace DaresGameBot.Save;

public sealed class GameStatsData
{
    [UsedImplicitly]
    public Dictionary<string, uint> Points { get; set; } = null!;
    [UsedImplicitly]
    public Dictionary<string, uint> Propositions { get; set; } = null!;
    [UsedImplicitly]
    public Dictionary<string, uint> Turns { get; set; } = null!;
}