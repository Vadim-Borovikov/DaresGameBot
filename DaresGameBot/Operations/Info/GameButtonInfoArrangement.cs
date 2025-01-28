using DaresGameBot.Game.Data;

namespace DaresGameBot.Operations.Info;

internal sealed class GameButtonInfoArrangement : GameButtonInfo
{
    public readonly Arrangement Arrangement;
    public readonly string Tag;

    public GameButtonInfoArrangement(Arrangement arrangement, string tag)
    {
        Arrangement = arrangement;
        Tag = tag;
    }
}