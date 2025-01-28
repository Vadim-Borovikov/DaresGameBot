using DaresGameBot.Game;

namespace DaresGameBot.Operations.Data.GameButtons;

internal sealed class GameButtonActionData : GameButtonData
{
    public readonly ActionInfo ActionInfo;

    public GameButtonActionData(ActionInfo actionInfo) => ActionInfo = actionInfo;
}