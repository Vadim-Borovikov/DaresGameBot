using DaresGameBot.Utilities.Extensions;

namespace DaresGameBot.Operations.Data.GameButtons;

internal sealed class ConfirmEndData
{
    internal enum ActionAfterGameEnds
    {
        UpdateCards,
        StartNewGame
    }

    public readonly ActionAfterGameEnds After;

    public static ConfirmEndData? From(string callbackQueryDataCore)
    {
        ActionAfterGameEnds? after = callbackQueryDataCore.ToActionAfterGameEnds();
        return after is null ? null : new ConfirmEndData(after.Value);
    }

    private ConfirmEndData(ActionAfterGameEnds after) => After = after;
}