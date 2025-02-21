// ReSharper disable NullableWarningSuppressionIsUsed

using System.Collections.Generic;
using JetBrains.Annotations;

namespace DaresGameBot.Save;

public sealed class PlayersRepositoryData
{
    [UsedImplicitly]
    public List<string> Names { get; init; } = null!;
    [UsedImplicitly]
    public Dictionary<string, PlayerData> Infos { get; init; } = null!;
    [UsedImplicitly]
    public int CurrentIndex { get; init; }
}