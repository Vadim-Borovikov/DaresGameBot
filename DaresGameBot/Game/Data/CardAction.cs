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
    [SheetField(PartnersToAssignTitle)]
    public byte PartnersToAssign;

    [UsedImplicitly]
    [Required]
    [SheetField(CompatablePartnersTitle)]
    public bool CompatablePartners;

    [UsedImplicitly]
    [Required]
    [SheetField(TagTitle)]
    public string Tag = null!;

    private const string PlayersTitle = "Минимум";
    private const string PartnersToAssignTitle = "Назначить";
    private const string CompatablePartnersTitle = "Партнёры должны совмещаться друг с другом";
    private const string TagTitle = "Символ";
}