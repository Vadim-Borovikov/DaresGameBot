using DaresGameBot.Game.Matchmaking.PlayerCheck;

namespace DaresGameBot.Operations.Data.PlayerListUpdates;

internal sealed class AddOrUpdatePlayerData : PlayerListUpdateData
{
    public readonly GroupChecker Checker;

    public AddOrUpdatePlayerData(string name, GroupChecker checker) : base(name) => Checker = checker;
}