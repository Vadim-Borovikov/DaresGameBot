using GoogleSheetsManager;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace DaresGameBot.Game.Data;

// ReSharper disable NullableWarningSuppressionIsUsed

internal class Card
{
    [UsedImplicitly]
    [Required]
    [SheetField("�����")]
    public string Description = null!;

    public bool Discarded;

    public virtual bool IsOkayFor(ushort playersAmount) => !Discarded;
}