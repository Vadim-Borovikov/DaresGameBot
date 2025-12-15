using JetBrains.Annotations;

namespace DaresGameBot.Game.States.Data;

public sealed class PlayerData
{
    [UsedImplicitly]
    public string Name { get; set; } = null!;

    [UsedImplicitly]
    public GroupsData GroupsData { get; set; } = null!;
    [UsedImplicitly]
    public bool Active { get; set; }
}