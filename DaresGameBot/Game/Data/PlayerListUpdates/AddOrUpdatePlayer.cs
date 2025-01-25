using DaresGameBot.Game.Matchmaking.PlayerCheck;

namespace DaresGameBot.Game.Data.PlayerListUpdates;

internal sealed class AddOrUpdatePlayer : PlayerListUpdate
{
    public readonly GroupChecker Checker;

    public AddOrUpdatePlayer(string name, GroupChecker checker) : base(name) => Checker = checker;
}