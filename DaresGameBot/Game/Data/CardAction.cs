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
    public byte Players;

    [UsedImplicitly]
    [Required]
    [SheetField("���������")]
    public byte PartnersToAssign;

    [UsedImplicitly]
    [Required]
    [SheetField("������")]
    public string Tag = null!;

    public override bool IsOkayFor(ushort playersAmount)
    {
        return (playersAmount >= Players) && base.IsOkayFor(playersAmount);
    }
}