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
    [Required]
    [SheetField(CompatablePartnersTitle)]
    public bool CompatablePartners;

    public ArrangementType ArrangementType;

    [UsedImplicitly]
    [SheetField(EquipmentTitle)]
    public string? Equipment;

    private const string TagTitle = "Символ";
    private const string PartnersTitle = "Партнёры";
    private const string CompatablePartnersTitle = "Партнёры должны совмещаться друг с другом";
    private const string EquipmentTitle = "Снаряжение";
}