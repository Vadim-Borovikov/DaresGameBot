using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Helpers;
using DaresGameBot.Game.Data.Players;
using GryphonUtilities.Extensions;
using DaresGameBot.Game.Matchmaking.PlayerCheck;
using DaresGameBot.Game.Matchmaking.Interactions;

namespace DaresGameBot.Game.Matchmaking;

internal sealed class DistributedMatchmaker : Matchmaker
{
    public DistributedMatchmaker(Compatibility compatibility, InteractionRepository interactionRepository)
        : base(compatibility)
    {
        _interactionRepository = interactionRepository;
    }

    public override IEnumerable<Player>? EnumerateMatches(Player player, IEnumerable<Player> all, byte amount,
        bool compatableWithEachOther)
    {
        List<Player> choices = EnumerateCompatiblePlayers(player, all).ToList();
        if (choices.Count < amount)
        {
            return null;
        }

        if (compatableWithEachOther)
        {
            IEnumerable<IReadOnlyList<Player>> groups = EnumerateIntercompatibleGroups(choices, amount);
            List<IReadOnlyList<Player>> bestGroups =
                groups.GroupBy(g => _interactionRepository.GetInteractions(player, g))
                      .OrderBy(g => g.Key)
                      .First()
                      .ToList();
            return RandomHelper.SelectItem(bestGroups);
        }

        List<Player> bestChoices = new();
        while (bestChoices.Count < amount)
        {
            int toAdd = amount - bestChoices.Count;

            List<Player> batch = choices.GroupBy(c => _interactionRepository.GetInteractions(player, c))
                                        .OrderBy(g => g.Key)
                                        .First()
                                        .ToList();

            if (batch.Count <= toAdd)
            {
                bestChoices.AddRange(batch);
                foreach (Player p in batch)
                {
                    choices.Remove(p);
                }
                continue;
            }

            IEnumerable<Player> selection = RandomHelper.EnumerateUniqueItems(batch, toAdd).Denull("Logic error");
            bestChoices.AddRange(selection);
            break;
        }
        return bestChoices;
    }

    private readonly InteractionRepository _interactionRepository;
}