// ReSharper disable NullableWarningSuppressionIsUsed

using JetBrains.Annotations;

namespace DaresGameBot.Game.States.Data;

public sealed class BotData
{
    [UsedImplicitly]
    public GameData? GameData { get; set; }

    [UsedImplicitly]
    public bool IncludeEn { get; set; }

    [UsedImplicitly]
    public int? PlayersMessageId { get; set; }

    [UsedImplicitly]
    public int? CardAdminMessageId { get; set; }

    [UsedImplicitly]
    public int? CardPlayerMessageId { get; set; }
}