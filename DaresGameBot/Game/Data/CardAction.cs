using GoogleSheetsManager;
using JetBrains.Annotations;
using System.ComponentModel.DataAnnotations;

namespace DaresGameBot.Game.Data;

// ReSharper disable NullableWarningSuppressionIsUsed

internal sealed class CardAction : Card
{
    [UsedImplicitly]
    [Required]
    [SheetField(PlayersTitle)]
    public byte Players;

    [UsedImplicitly]
    [Required]
    [SheetField(PartnersTitle)]
    public byte Partners;

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

    private const string PlayersTitle = "�������";
    private const string PartnersTitle = "�������";
    private const string AssignPartnersTitle = "�������� ��������� ���";
    private const string CompatablePartnersTitle = "������� ������ ����������� ���� � ������";
    private const string HelpersTitle = "���������";
    private const string TagTitle = "������";
}