using GoogleSheetsManager;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace DaresGameBot.Game.Data.Cards;

// ReSharper disable NullableWarningSuppressionIsUsed

internal sealed class Question
{
    [UsedImplicitly]
    [Required]
    [SheetField(DescriptionTitle)]
    public string Description = null!;

    [UsedImplicitly]
    [SheetField(DescriptionEnTitle)]
    public string DescriptionEn = null!;

    private const string DescriptionTitle = "�����";
    private const string DescriptionEnTitle = "Text";
}