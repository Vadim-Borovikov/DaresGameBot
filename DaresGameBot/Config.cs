using AbstractBot;
using System.ComponentModel.DataAnnotations;

// ReSharper disable NullableWarningSuppressionIsUsed

namespace DaresGameBot;

public class Config : ConfigGoogleSheets
{
    [Required]
    [Range(1, ushort.MaxValue)]
    public ushort InitialPlayersAmount { get; init; }

    [Required]
    [Range(0.0f, 1.0f)]
    public float InitialChoiceChance { get; init; }

    [Required]
    [MinLength(1)]
    public string ActionsGoogleRange { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string QuestionsGoogleRange { get; init; } = null!;
}