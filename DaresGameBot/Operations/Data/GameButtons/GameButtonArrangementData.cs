using DaresGameBot.Game;

namespace DaresGameBot.Operations.Data.GameButtons;

internal sealed class GameButtonArrangementData : GameButtonData
{
    public readonly Arrangement Arrangement;
    public readonly string Tag;

    public GameButtonArrangementData(Arrangement arrangement, string tag)
    {
        Arrangement = arrangement;
        Tag = tag;
    }
}