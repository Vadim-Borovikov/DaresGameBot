using GoogleSheetsManager;
using JetBrains.Annotations;
using System.ComponentModel.DataAnnotations;

namespace DaresGameBot.Game.Data;

// ReSharper disable NullableWarningSuppressionIsUsed

internal sealed class CardAction : Card
{
    [UsedImplicitly]
    [Required]
    [SheetField("Минимум")]
    public ushort Players;

    [UsedImplicitly]
    [Required]
    [SheetField("Назначить")]
    public ushort PartnersToAssign;

    [UsedImplicitly]
    [Required]
    [SheetField("Символ")]
    public string Tag = null!;

    public override bool IsOkayFor(ushort playersAmount)
    {
        return (playersAmount >= Players) && base.IsOkayFor(playersAmount);
    }
}