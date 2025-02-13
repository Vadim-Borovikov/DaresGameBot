using DaresGameBot.Game;

namespace DaresGameBot.Operations.Data.GameButtons;

internal sealed class CompleteQuestionData : CompleteCardData
{
    public readonly ushort Id;
    public CompleteQuestionData(ushort id, Arrangement? declinedArrangement = null)
        : base(declinedArrangement)
    {
        Id = id;
    }
}