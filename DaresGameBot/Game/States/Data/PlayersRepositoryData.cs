// ReSharper disable NullableWarningSuppressionIsUsed

using System.Collections.Generic;
using JetBrains.Annotations;

namespace DaresGameBot.Game.States.Data;

public sealed class PlayersRepositoryData
{
    [UsedImplicitly]
    public List<string> Ids { get; init; } = null!;
    [UsedImplicitly]
    public Dictionary<string, PlayerData> Infos { get; init; } = null!;
    [UsedImplicitly]
    public int CurrentIndex { get; init; }
}