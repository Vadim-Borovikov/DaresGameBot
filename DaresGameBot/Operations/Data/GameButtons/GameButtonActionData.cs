using DaresGameBot.Game;

namespace DaresGameBot.Operations.Data.GameButtons;

internal sealed class GameButtonActionData : GameButtonData
{
    public readonly ActionInfo ActionInfo;
    public readonly string Tag;

    public GameButtonActionData(ActionInfo actionInfo, string tag)
    {
        ActionInfo = actionInfo;
        Tag = tag;
    }
}