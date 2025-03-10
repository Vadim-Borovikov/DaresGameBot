using System.Collections.Generic;
using JetBrains.Annotations;

// ReSharper disable NullableWarningSuppressionIsUsed

namespace DaresGameBot.Game.States.Data;

public sealed class GroupsData
{
    [UsedImplicitly]
    public string Group { get; set; } = null!;
    [UsedImplicitly]
    public List<string> CompatableGroups { get; set; } = null!;
}