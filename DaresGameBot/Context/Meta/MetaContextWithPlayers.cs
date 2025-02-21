namespace DaresGameBot.Context.Meta;

internal sealed class GameStatsMetaContext : MetaContext
{
    public readonly PlayersRepository Players;

    public GameStatsMetaContext(MetaContext meta, PlayersRepository players)
        : base(meta.ActionOptions, meta.Actions, meta.Questions)
    {
        Players = players;
    }
}