using GoogleSheetsManager;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace DaresGameBot.Game.Data;

// ReSharper disable NullableWarningSuppressionIsUsed

internal class CardData
{
    [UsedImplicitly]
    [Required]
    [SheetField(DescriptionTitle)]
    public string Description = null!;

    [UsedImplicitly]
    [SheetField(DescriptionEnTitle)]
    public string DescriptionEn = null!;

    private const string DescriptionTitle = "Текст";
    private const string DescriptionEnTitle = "Text";
}