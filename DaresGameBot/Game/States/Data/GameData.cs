// ReSharper disable NullableWarningSuppressionIsUsed

using System.Collections.Generic;
using JetBrains.Annotations;

namespace DaresGameBot.Game.States.Data;

public sealed class GameData
{
    [UsedImplicitly]
    public Dictionary<ushort, uint> ActionUses { get; set; } = null!;
    [UsedImplicitly]
    public Dictionary<ushort, uint> QuestionUses { get; set; } = null!;

    [UsedImplicitly]
    public PlayersRepositoryData PlayersRepositoryData { get; set; } = null!;
    [UsedImplicitly]
    public GameStatsData GameStatsData { get; set; } = null!;

    [UsedImplicitly]
    public string? CurrentState { get; set; }

    [UsedImplicitly]
    public ArrangementData? CurrentArrangementData { get; set; }

    [UsedImplicitly]
    public ushort? CurrentCardId { get; set; }
    [UsedImplicitly]
    public string? CurrentCardTag { get; set; }


    [UsedImplicitly]
    public ushort? RevealedQuestionId { get; set; }

    [UsedImplicitly]
    public string? ActionsVersion { get; set; }

    [UsedImplicitly]
    public string? QuestionsVersion { get; set; }
}