using System.Collections.Generic;
using DaresGameBot.Configs;

namespace DaresGameBot.Game.States.Cores;

internal sealed class GameStatsStateCore
{
    public readonly Dictionary<string, Option> ActionOptions;
    public readonly ushort? QuestionPoints;
    public readonly PlayersRepository Players;

    public GameStatsStateCore(Dictionary<string, Option> actionOptions, ushort? questionPoints,
        PlayersRepository players)
    {
        ActionOptions = actionOptions;
        Players = players;
        QuestionPoints = questionPoints;
    }
}