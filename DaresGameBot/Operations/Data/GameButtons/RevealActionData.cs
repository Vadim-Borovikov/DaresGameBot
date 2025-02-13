using DaresGameBot.Game;

namespace DaresGameBot.Operations.Data.GameButtons;

internal sealed class RevealActionData : RevealCardData
{
    public readonly string Tag;

    public RevealActionData(Arrangement arrangement, string tag) : base(arrangement) => Tag = tag;
}