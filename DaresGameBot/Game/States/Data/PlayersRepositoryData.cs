// ReSharper disable NullableWarningSuppressionIsUsed

using System.Collections.Generic;
using JetBrains.Annotations;

namespace DaresGameBot.Game.States.Data;

public sealed class PlayersRepositoryData
{
    [UsedImplicitly]
    public List<string> IdsOld { get; init; } = null!;
    [UsedImplicitly]
    public Dictionary<string, PlayerData> InfosOld { get; init; } = null!;
    [UsedImplicitly]
    public int CurrentIndexOld { get; init; }
    [UsedImplicitly]
    public Dictionary<long, PlayerData> Infos { get; init; } = null!;
    [UsedImplicitly]
    public long? Current { get; init; }
}