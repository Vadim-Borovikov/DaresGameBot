using System;

namespace DaresGameBot.Operations.Data.GameButtons;

internal sealed class EndGameData : GameButtonData
{
    internal enum ActionAfterGameEnds
    {
        UpdateCards,
        StartNewGame
    }

    public readonly ActionAfterGameEnds After;

    public static EndGameData? From(string callbackQueryDataCore)
    {
        return Enum.TryParse(callbackQueryDataCore, out ActionAfterGameEnds after) ? new EndGameData(after) : null;
    }

    private EndGameData(ActionAfterGameEnds after) => After = after;
}