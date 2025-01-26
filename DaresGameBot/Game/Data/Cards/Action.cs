using GoogleSheetsManager;
using JetBrains.Annotations;
using System.ComponentModel.DataAnnotations;

namespace DaresGameBot.Game.Data.Cards;

// ReSharper disable NullableWarningSuppressionIsUsed

internal sealed class Action
{
    [UsedImplicitly]
    [Required]
    [SheetField(Description0Title)]
    public string Description0 = null!;

    [UsedImplicitly]
    [SheetField(Description0EnTitle)]
    public string Description0En = null!;

    [UsedImplicitly]
    [Required]
    [SheetField(Description1Title)]
    public string Description1 = null!;

    [UsedImplicitly]
    [SheetField(Description1EnTitle)]
    public string Description1En = null!;

    [UsedImplicitly]
    [Required]
    [SheetField(Description2Title)]
    public string Description2 = null!;

    [UsedImplicitly]
    [SheetField(Description2EnTitle)]
    public string Description2En = null!;

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

    private const string ImageTitle = "Картинка";
    private const string PartnersTitle = "Партнёры";
    private const string CompatablePartnersTitle = "Партнёры должны совмещаться друг с другом";
    private const string Description0Title = "Текст 🤗";
    private const string Description0EnTitle = "Text 🤗";
    private const string Description1Title = "Текст 😘";
    private const string Description1EnTitle = "Text 😘";
    private const string Description2Title = "Текст 🔥";
    private const string Description2EnTitle = "Text 🔥";
}