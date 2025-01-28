using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Helpers;
using DaresGameBot.Game.Matchmaking.Interactions;
using DaresGameBot.Game.Players;

namespace DaresGameBot.Game.Matchmaking;

internal sealed class DistributedMatchmaker : Matchmaker
{
    public DistributedMatchmaker(Repository players, IReadOnlyDictionary<string, ushort> points)
        : base(players)
    {
        _players = players;
        _interactionRepository = new InteractionRepository(points);
    }

    public override void OnInteractionPurposed(string player, Arrangement arrangement)
    {
        _interactionRepository.OnInteractionPurposed(player, arrangement);
    }
    public override void OnInteractionCompleted(string player, Arrangement arrangement, string tag)
    {
        _interactionRepository.OnInteractionCompleted(player, arrangement, tag);
    }

    public override IEnumerable<string>? EnumerateMatches(string player, IEnumerable<string> all, byte amount,
        bool compatableWithEachOther)
    {
        IEnumerable<string> choices = EnumerateCompatablePlayers(player, all);
        string[] shuffled = RandomHelper.Shuffle(choices);
        if (shuffled.Length < amount)
        {
            return null;
        }

        if (compatableWithEachOther)
        {
            IEnumerable<IReadOnlyList<string>> groups = EnumerateIntercompatableGroups(shuffled, amount);
            return groups.OrderBy(g => _interactionRepository.GetInteractions(player, g, false))
                         .ThenByDescending(g => _interactionRepository.GetInteractions(player, g, true))
                         .ThenBy(g => g.Sum(p => _players.GetPoints(p)))
                         .First();
        }

        return shuffled.OrderBy(p => _interactionRepository.GetInteractions(player, p, false))
                       .ThenByDescending(p => _interactionRepository.GetInteractions(player, p, true))
                       .ThenBy(_players.GetPoints)
                       .Take(amount)
                       .ToList();
    }

    private readonly InteractionRepository _interactionRepository;
    private readonly Repository _players;
}