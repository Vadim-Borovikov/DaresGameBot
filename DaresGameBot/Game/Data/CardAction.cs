using GoogleSheetsManager;
using JetBrains.Annotations;
using System.ComponentModel.DataAnnotations;

namespace DaresGameBot.Game.Data;

// ReSharper disable NullableWarningSuppressionIsUsed

internal sealed class CardAction : Card
{
    [UsedImplicitly]
    [Required]
    [SheetField("�������")]
    public ushort Players;

    [UsedImplicitly]
    [Required]
    [SheetField("���������")]
    public ushort PartnersToAssign;

    [UsedImplicitly]
    [Required]
    [SheetField("������")]
    public string Tag = null!;
}