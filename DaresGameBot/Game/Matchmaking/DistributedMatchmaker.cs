using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Game.Matchmaking.Compatibility;
using DaresGameBot.Game.Data;
using DaresGameBot.Utilities;
using DaresGameBot.Game.States;

namespace DaresGameBot.Game.Matchmaking;

internal sealed class DistributedMatchmaker : Matchmaker
{
    public DistributedMatchmaker(PlayersRepository players, GameStats gameStats, ICompatibility compatibility)
        : base(players, compatibility)
    {
        _gameStats = gameStats;
    }

    protected override IEnumerable<string>? EnumerateMatches(ArrangementType arrangementType)
    {
        IEnumerable<string> choices = EnumerateCompatablePlayers();
        string[] shuffled = RandomHelper.Shuffle(choices);
        if (shuffled.Length < arrangementType.Partners)
        {
            return null;
        }

        if (!arrangementType.CompatablePartners)
        {
            return shuffled.OrderBy(p => _gameStats.GetPropositions(Players.Current, p))
                           .ThenBy(_gameStats.GetPropositions)
                           .Take(arrangementType.Partners);
        }

        IEnumerable<IReadOnlyList<string>> groups = EnumerateIntercompatableGroups(shuffled, arrangementType.Partners);
        return groups.OrderBy(g => _gameStats.GetPropositions(Players.Current, g))
                     .ThenBy(g => g.Sum(p => _gameStats.GetPropositions(p)))
                     .First();
    }

    private readonly GameStats _gameStats;
}