using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Game.Data;
using DaresGameBot.Game.Matchmaking.Compatibility;
using DaresGameBot.Helpers;
using DaresGameBot.Game.Players;

namespace DaresGameBot.Game.Matchmaking;

internal sealed class DistributedMatchmaker : Matchmaker
{
    public DistributedMatchmaker(Repository players, GameStats gameStats, ICompatibility compatibility)
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

        if (arrangementType.CompatablePartners)
        {
            IEnumerable<IReadOnlyList<string>> groups =
                EnumerateIntercompatableGroups(shuffled, arrangementType.Partners);
            return groups.OrderBy(g => _gameStats.GetInteractions(Players.Current, g, false))
                         .ThenByDescending(g => _gameStats.GetInteractions(Players.Current, g, true))
                         .ThenBy(g => g.Sum(p => _gameStats.Points[p]))
                         .First();
        }

        return shuffled.OrderBy(p => _gameStats.GetInteractions(Players.Current, p, false))
                       .ThenByDescending(p => _gameStats.GetInteractions(Players.Current, p, true))
                       .ThenBy(p => _gameStats.Points[p])
                       .Take(arrangementType.Partners)
                       .ToList();
    }

    private readonly GameStats _gameStats;
}