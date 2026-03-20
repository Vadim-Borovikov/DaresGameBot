using System.Collections.Generic;
using JetBrains.Annotations;

namespace DaresGameBot.Game.States.Data;

public sealed class PlayerData
{
    [UsedImplicitly]
    public string? Username { get; set; }

    [UsedImplicitly]
    public List<byte> Rounds { get; set; } = null!;

    [UsedImplicitly]
    public GroupsData GroupsData { get; set; } = null!;
    [UsedImplicitly]
    public bool Active { get; set; }
}