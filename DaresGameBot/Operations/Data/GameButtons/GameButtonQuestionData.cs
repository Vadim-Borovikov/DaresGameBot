namespace DaresGameBot.Operations.Data.GameButtons;

internal sealed class GameButtonQuestionData : GameButtonData
{
    public readonly ushort Id;
    public GameButtonQuestionData(ushort id) => Id = id;
}