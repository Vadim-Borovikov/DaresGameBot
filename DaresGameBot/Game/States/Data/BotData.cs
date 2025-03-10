// ReSharper disable NullableWarningSuppressionIsUsed

using AbstractBot.Modules.Context.Localization;
using AbstractBot.Modules.Context;
using JetBrains.Annotations;

namespace DaresGameBot.Game.States.Data;

public sealed class BotData : BotStateData<LocalizationUserStateData>
{
    [UsedImplicitly]
    public GameData? GameData { get; set; }

    [UsedImplicitly]
    public int? PlayersMessageId { get; set; }

    [UsedImplicitly]
    public int? CardAdminMessageId { get; set; }

    [UsedImplicitly]
    public int? CardPlayerMessageId { get; set; }
}