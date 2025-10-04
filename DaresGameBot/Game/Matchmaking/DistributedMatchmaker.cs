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
        List<string> choices =
            EnumerateCompatablePlayers().OrderBy(p => _gameStats.GetPropositions(Players.Current, p))
                                        .ThenBy(_gameStats.GetPropositions)
                                        .ThenByShuffled()
                                        .ToList();

        if (choices.Count < arrangementType.Partners)
        {
            return null;
        }

        return arrangementType.CompatablePartners
            ? EnumerateIntercompatableGroups(choices, arrangementType.Partners).FirstOrDefault()
            : choices.Take(arrangementType.Partners);
    }

    private readonly GameStats _gameStats;
}