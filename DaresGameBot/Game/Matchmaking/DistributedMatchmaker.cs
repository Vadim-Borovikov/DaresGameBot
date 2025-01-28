﻿using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Helpers;
using GryphonUtilities.Extensions;
using DaresGameBot.Game.Matchmaking.PlayerCheck;
using DaresGameBot.Game.Matchmaking.Interactions;

namespace DaresGameBot.Game.Matchmaking;

internal sealed class DistributedMatchmaker : Matchmaker
{
    public DistributedMatchmaker(ICompatibility compatibility) : base(compatibility)
    {
        _interactionRepository = new InteractionRepository();
    }

    public override void OnInteraction(string player, Arrangement arrangement, string tag)
    {
        _interactionRepository.OnInteraction(player, arrangement, tag);
    }

    public override IEnumerable<string>? EnumerateMatches(string player, IEnumerable<string> all, byte amount,
        bool compatableWithEachOther)
    {
        List<string> choices = EnumerateCompatablePlayers(player, all).ToList();
        if (choices.Count < amount)
        {
            return null;
        }

        if (compatableWithEachOther)
        {
            IEnumerable<IReadOnlyList<string>> groups = EnumerateIntercompatableGroups(choices, amount);
            List<IReadOnlyList<string>> bestGroups =
                groups.GroupBy(g => _interactionRepository.GetInteractions(player, g))
                      .OrderBy(g => g.Key)
                      .First()
                      .ToList();
            return RandomHelper.SelectItem(bestGroups);
        }

        List<string> bestChoices = new();
        while (bestChoices.Count < amount)
        {
            int toAdd = amount - bestChoices.Count;

            List<string> batch = choices.GroupBy(c => _interactionRepository.GetInteractions(player, c))
                                        .OrderBy(g => g.Key)
                                        .First()
                                        .ToList();

            if (batch.Count <= toAdd)
            {
                bestChoices.AddRange(batch);
                foreach (string p in batch)
                {
                    choices.Remove(p);
                }
                continue;
            }

            IEnumerable<string> selection = RandomHelper.EnumerateUniqueItems(batch, toAdd).Denull("Logic error");
            bestChoices.AddRange(selection);
            break;
        }
        return bestChoices;
    }

    private readonly InteractionRepository _interactionRepository;
}