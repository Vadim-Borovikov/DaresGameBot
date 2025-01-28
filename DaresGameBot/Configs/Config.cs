using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AbstractBot.Configs;
using JetBrains.Annotations;

// ReSharper disable NullableWarningSuppressionIsUsed

namespace DaresGameBot.Configs;

public class Config : ConfigWithSheets<Texts>
{
    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public string GoogleSheetId { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    [Range(0.0, 1.0)]
    public decimal InitialChoiceChance { get; init; }

    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public string ActionsRange { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public string QuestionsRange { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    public long LogsChatId { get; init; }

    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public string ImagesFolder { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public List<Option> ActionOptions { get; init; } = null!;

    internal IReadOnlyDictionary<string, ushort> Options => ActionOptions.ToDictionary(o => o.Tag, o => o.Points);
}