// ReSharper disable NullableWarningSuppressionIsUsed

using AbstractBot.Modules.Context;
using JetBrains.Annotations;

namespace DaresGameBot.Game.States.Data;

internal sealed class BotData : BotStateData<UserStateData>
{
    [UsedImplicitly]
    public GameData? GameData { get; set; }

    [UsedImplicitly]
    public int? PlayersMessageId { get; set; }
    [UsedImplicitly]
    public string? CurentPinState { get; set; }
}