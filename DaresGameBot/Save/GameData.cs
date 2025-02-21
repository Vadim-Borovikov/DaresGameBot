﻿// ReSharper disable NullableWarningSuppressionIsUsed

using System.Collections.Generic;
using JetBrains.Annotations;

namespace DaresGameBot.Save;

public sealed class GameData
{
    [UsedImplicitly]
    public Dictionary<ushort, uint> ActionUses { get; set; } = null!;
    [UsedImplicitly]
    public Dictionary<ushort, uint> QuestionUses { get; set; } = null!;

    [UsedImplicitly]
    public PlayersRepositoryData PlayersRepositoryData { get; set; } = null!;
    [UsedImplicitly]
    public GameStatsData GameStatsData { get; set; } = null!;

    [UsedImplicitly]
    public bool IncludeEn { get; set; }

    [UsedImplicitly]
    public int? PlayersMessageId { get; set; }

    [UsedImplicitly]
    public string? CurrentState { get; set; }
}