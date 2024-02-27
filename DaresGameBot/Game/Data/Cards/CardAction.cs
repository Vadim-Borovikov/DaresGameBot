using GoogleSheetsManager;
using JetBrains.Annotations;
using System.ComponentModel.DataAnnotations;

namespace DaresGameBot.Game.Data.Cards;

// ReSharper disable NullableWarningSuppressionIsUsed

internal sealed class CardAction : Card
{
    public ushort Id = 0;

    [UsedImplicitly]
    [Required]
    [SheetField(PartnersTitle)]
    public byte Partners;

    [UsedImplicitly]
    [SheetField(ImageTitle)]
    public string? ImagePath;

    [UsedImplicitly]
    [Required]
    [SheetField(AssignPartnersTitle)]
    public bool AssignPartners;

    [UsedImplicitly]
    [Required]
    [SheetField(CompatablePartnersTitle)]
    public bool CompatablePartners;

    [UsedImplicitly]
    [Required]
    [SheetField(HelpersTitle)]
    public byte Helpers;

    [UsedImplicitly]
    [Required]
    [SheetField(TagTitle)]
    public string Tag = null!;

    private const string ImageTitle = "��������";
    private const string PartnersTitle = "�������";
    private const string AssignPartnersTitle = "�������� ��������� ���";
    private const string CompatablePartnersTitle = "������� ������ ����������� ���� � ������";
    private const string HelpersTitle = "���������";
    private const string TagTitle = "������";
}