using GoogleSheetsManager;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace DaresGameBot.Game.Data.Cards;

// ReSharper disable NullableWarningSuppressionIsUsed

internal class Card
{
    [UsedImplicitly]
    [Required]
    [SheetField(DescriptionTitle)]
    public string Description = null!;

    [UsedImplicitly]
    [SheetField(DescriptionEnTitle)]
    public string? DescriptionEn;

    private const string DescriptionTitle = "Текст";
    private const string DescriptionEnTitle = "Text";
}