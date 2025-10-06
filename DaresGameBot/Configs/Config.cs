using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AbstractBot.Interfaces.Modules.Config;
using AbstractBot.Models.Config;
using DaresGameBot.Game.Data;
using JetBrains.Annotations;

// ReSharper disable NullableWarningSuppressionIsUsed

namespace DaresGameBot.Configs;

public class Config : ConfigWithSheets, ILocalizationConfig<Texts>
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
    [MinLength(1)]
    public string ImagesFolder { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public string SavePath { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public Dictionary<string, Option> ActionOptions { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    public long AdminChatId { get; init; }
    [UsedImplicitly]
    [Required]
    public long PlayerChatId { get; init; }

    [UsedImplicitly]
    [Required]
    public string DefaultLanguageCode { get; init; } = null!;

    [UsedImplicitly]
    public Dictionary<string, Texts> AllTexts { get; set; } = new();

    [UsedImplicitly]
    [Required]
    public Dictionary<string, string> Images { get; set; } = new();

    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public string ImagesKeySeparator { get; set; } = null!;

    internal string? GetArrangementImage(ArrangementType type)
    {
        string key = $"{type.Partners}{ImagesKeySeparator}{type.CompatablePartners}";
        return Images.GetValueOrDefault(key);
    }
}