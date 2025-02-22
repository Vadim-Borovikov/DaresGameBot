// ReSharper disable NullableWarningSuppressionIsUsed

using AbstractBot;
using JetBrains.Annotations;

namespace DaresGameBot.Save;

public sealed class Data : SaveData<object>
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