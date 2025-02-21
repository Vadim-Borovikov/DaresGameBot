using JetBrains.Annotations;

namespace DaresGameBot.Save;

public sealed class PlayerData
{
    [UsedImplicitly]
    public GroupsData GroupInfo { get; set; } = null!;
    [UsedImplicitly]
    public bool Active { get; set; }
}