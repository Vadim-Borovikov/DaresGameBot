using DaresGameBot.Game.Data;

namespace DaresGameBot.Operations.Info;

internal sealed class GameButtonInfoArrangement : GameButtonInfo
{
    public readonly ArrangementInfo ArrangementInfo;
    public readonly string Tag;

    public GameButtonInfoArrangement(ArrangementInfo arrangementInfo, string tag)
    {
        ArrangementInfo = arrangementInfo;
        Tag = tag;
    }
}