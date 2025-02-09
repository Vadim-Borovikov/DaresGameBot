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

        if (!arrangementType.CompatablePartners)
        {
            return shuffled.OrderBy(p => _gameStats.GetPropositions(Players.Current, p))
                           .Take(arrangementType.Partners);
        }

        IEnumerable<IReadOnlyList<string>> groups = EnumerateIntercompatableGroups(shuffled, arrangementType.Partners);
        return groups.OrderBy(g => _gameStats.GetPropositions(Players.Current, g))
                     .First();
    }

    private readonly GameStats _gameStats;
}