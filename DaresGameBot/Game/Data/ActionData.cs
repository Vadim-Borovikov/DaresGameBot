using System.Collections.Generic;
using GoogleSheetsManager;
using JetBrains.Annotations;
using System.ComponentModel.DataAnnotations;

namespace DaresGameBot.Game.Data;

// ReSharper disable NullableWarningSuppressionIsUsed

internal sealed class ActionData
{
    [UsedImplicitly]
    [Required]
    [SheetField(PartnersTitle)]
    public byte Partners;

    [UsedImplicitly]
    [Required]
    [SheetField(CompatablePartnersTitle)]
    public bool CompatablePartners;

    public ArrangementType ArrangementType;

    public readonly Dictionary<string, (string ru, string en)> Descriprions = new();

    [UsedImplicitly]
    [SheetField(EquipmentTitle)]
    public string? Equipment;

    [UsedImplicitly]
    [Required]
    [SheetField(DescriptionTitle1)]
    public string Description1 = null!;

    [UsedImplicitly]
    [SheetField(DescriptionEnTitle1)]
    public string DescriptionEn1 = null!;

    [UsedImplicitly]
    [Required]
    [SheetField(DescriptionTitle2)]
    public string Description2 = null!;

    [UsedImplicitly]
    [SheetField(DescriptionEnTitle2)]
    public string DescriptionEn2 = null!;

    private const string DescriptionTitle1 = "😏 Текст";
    private const string DescriptionEnTitle1 = "😏 Text";
    private const string DescriptionTitle2 = "🔥 Текст";
    private const string DescriptionEnTitle2 = "🔥 Text";
    private const string PartnersTitle = "Партнёры";
    private const string CompatablePartnersTitle = "Партнёры должны совмещаться друг с другом";
    private const string EquipmentTitle = "Снаряжение";
}