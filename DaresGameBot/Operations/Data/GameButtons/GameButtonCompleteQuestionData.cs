namespace DaresGameBot.Operations.Data.GameButtons;

internal sealed class GameButtonCompleteQuestionData : GameButtonData
{
    public readonly ushort Id;
    public GameButtonCompleteQuestionData(ushort id) => Id = id;
}