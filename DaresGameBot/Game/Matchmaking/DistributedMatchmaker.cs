using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Game.Data;
using DaresGameBot.Game.Matchmaking.Compatibility;
using DaresGameBot.Helpers;
using DaresGameBot.Game.Matchmaking.Interactions;
using DaresGameBot.Game.Players;

namespace DaresGameBot.Game.Matchmaking;

internal sealed class DistributedMatchmaker : Matchmaker
{
    public DistributedMatchmaker(Repository players, PointsManager pointsManager, ICompatibility compatibility)
        : base(players, compatibility)
    {
        _interactionRepository = new InteractionRepository(pointsManager);
    }

    public override void OnInteractionPurposed(string player, Arrangement arrangement)
    {
        _interactionRepository.OnInteractionPurposed(player, arrangement);
    }
    public override void OnInteractionCompleted(string player, Arrangement arrangement, string tag,
        bool completedFully)
    {
        _interactionRepository.OnInteractionCompleted(player, arrangement, tag, completedFully);
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
            return groups.OrderBy(g => _interactionRepository.GetInteractions(Players.Current, g, false))
                         .ThenByDescending(g => _interactionRepository.GetInteractions(Players.Current, g, true))
                         .ThenBy(g => g.Sum(p => Players.GetPoints(p)))
                         .First();
        }

        return shuffled.OrderBy(p => _interactionRepository.GetInteractions(Players.Current, p, false))
                       .ThenByDescending(p => _interactionRepository.GetInteractions(Players.Current, p, true))
                       .ThenBy(Players.GetPoints)
                       .Take(arrangementType.Partners)
                       .ToList();
    }

    private readonly InteractionRepository _interactionRepository;
}