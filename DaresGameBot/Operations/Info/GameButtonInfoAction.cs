using DaresGameBot.Game.Data;

namespace DaresGameBot.Operations.Info;

internal sealed class GameButtonInfoAction : GameButtonInfo
{
    public readonly ActionInfo ActionInfo;
    public readonly string Tag;

    public GameButtonInfoAction(ActionInfo actionInfo, string tag)
    {
        ActionInfo = actionInfo;
        Tag = tag;
    }
}