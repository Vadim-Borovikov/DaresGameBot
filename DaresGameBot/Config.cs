using AbstractBot;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

// ReSharper disable NullableWarningSuppressionIsUsed

namespace DaresGameBot;

public class Config : ConfigGoogleSheets
{
    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public string GoogleSheetId { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    [Range(1, ushort.MaxValue)]
    public ushort InitialPlayersAmount { get; init; }

    [UsedImplicitly]
    [Required]
    [Range(0.0f, 1.0f)]
    public float InitialChoiceChance { get; init; }

    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public string ActionsGoogleRange { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public string QuestionsGoogleRange { get; init; } = null!;
}