namespace DaresGameBot.Operations.Data.GameButtons;

internal sealed class CompleteQuestionData : CompleteCardData
{
    public readonly ushort Id;
    public CompleteQuestionData(ushort id) => Id = id;
}