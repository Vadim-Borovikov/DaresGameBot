// ReSharper disable NullableWarningSuppressionIsUsed

using System.Collections.Generic;
using JetBrains.Annotations;

namespace DaresGameBot.Game.States.Data;

public sealed class PlayersRepositoryData
{
    [UsedImplicitly]
    public List<long> Ids { get; init; } = null!;
    [UsedImplicitly]
    public Dictionary<long, PlayerData> Infos { get; init; } = null!;
    [UsedImplicitly]
    public int CurrentIndex { get; init; }
}