using System.Collections.Generic;
using DaresGameBot.Configs;
using DaresGameBot.Game.Data;

namespace DaresGameBot.Game.States.Cores;

internal sealed class GameStatsStateCore
{
    public readonly Dictionary<string, Option> ActionOptions;
    public readonly ushort? QuestionPoints;
    public readonly Dictionary<ushort, ActionData> Actions;
    public readonly PlayersRepository Players;

    public GameStatsStateCore(Dictionary<string, Option> actionOptions, ushort? questionPoints,
        Dictionary<ushort, ActionData> actions, PlayersRepository players)
    {
        ActionOptions = actionOptions;
        Actions = actions;
        Players = players;
        QuestionPoints = questionPoints;
    }
}