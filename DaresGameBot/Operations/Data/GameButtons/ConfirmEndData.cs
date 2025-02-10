using System;

namespace DaresGameBot.Operations.Data.GameButtons;

internal sealed class ConfirmEndData : GameButtonData
{
    internal enum ActionAfterGameEnds
    {
        UpdateCards,
        StartNewGame
    }

    public readonly ActionAfterGameEnds After;

    public static ConfirmEndData? From(string callbackQueryDataCore)
    {
        return Enum.TryParse(callbackQueryDataCore, out ActionAfterGameEnds after) ? new ConfirmEndData(after) : null;
    }

    private ConfirmEndData(ActionAfterGameEnds after) => After = after;
}