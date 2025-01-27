using GoogleSheetsManager;
using JetBrains.Annotations;
using System.ComponentModel.DataAnnotations;

namespace DaresGameBot.Game.Data.Cards;

// ReSharper disable NullableWarningSuppressionIsUsed

internal sealed class Action
{
    [UsedImplicitly]
    [Required]
    [SheetField(TagTitle)]
    public string Tag = null!;

    [UsedImplicitly]
    [Required]
    [SheetField(DescriptionTitle)]
    public string Description = null!;

    [UsedImplicitly]
    [SheetField(DescriptionEnTitle)]
    public string DescriptionEn = null!;

    [UsedImplicitly]
    [Required]
    [SheetField(PartnersTitle)]
    public byte Partners;

    [UsedImplicitly]
    [SheetField(ImageTitle)]
    public string? ImagePath;

    [UsedImplicitly]
    [Required]
    [SheetField(CompatablePartnersTitle)]
    public bool CompatablePartners;

    public Arrangement Arrangement = null!;

    private const string TagTitle = "Символ";
    private const string DescriptionTitle = "Текст";
    private const string DescriptionEnTitle = "Text";
    private const string ImageTitle = "Картинка";
    private const string PartnersTitle = "Партнёры";
    private const string CompatablePartnersTitle = "Партнёры должны совмещаться друг с другом";
}