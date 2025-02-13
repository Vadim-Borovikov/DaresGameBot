using DaresGameBot.Game;

namespace DaresGameBot.Operations.Data.GameButtons;

internal sealed class CompleteActionData : CompleteCardData
{
    public readonly ActionInfo ActionInfo;
    public readonly bool CompletedFully;

    public CompleteActionData(ActionInfo actionInfo, bool completedFully) : base(actionInfo.Arrangement)
    {
        ActionInfo = actionInfo;
        CompletedFully = completedFully;
    }
}