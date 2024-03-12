namespace DaresGameBot.Operations.Info;

internal sealed class UpdateNameInfo
{
    public readonly Game.Data.Game Game;
    public readonly string Name;

    public UpdateNameInfo(Game.Data.Game game, string name)
    {
        Game = game;
        Name = name;
    }
}