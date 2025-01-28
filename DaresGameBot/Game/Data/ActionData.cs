using GoogleSheetsManager;
using JetBrains.Annotations;
using System.ComponentModel.DataAnnotations;

namespace DaresGameBot.Game.Data;

// ReSharper disable NullableWarningSuppressionIsUsed

internal sealed class ActionData : CardData
{
    [UsedImplicitly]
    [Required]
    [SheetField(TagTitle)]
    public string Tag = null!;

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

    public ArrangementType ArrangementType;

    private const string TagTitle = "Символ";
    private const string ImageTitle = "Картинка";
    private const string PartnersTitle = "Партнёры";
    private const string CompatablePartnersTitle = "Партнёры должны совмещаться друг с другом";
}