namespace DaresGameBot.Operations.Info;

internal sealed class GameButtonInfoQuestion : GameButtonInfo
{
    public readonly string Player;

    public GameButtonInfoQuestion(string player) => Player = player;
}