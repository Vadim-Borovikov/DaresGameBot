﻿using System;
using DaresGameBot.Game.Data;
using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Helpers;

namespace DaresGameBot.Game.Matchmaking;

internal sealed class RandomMatchmaker : Matchmaker
{
    public RandomMatchmaker(Compatibility compatibility, Random? random = null) : base(compatibility)
    {
        _random = random ?? Random.Shared;
    }

    public override IEnumerable<Player>? EnumerateMatches(Player player, IEnumerable<Player> all, byte amount,
        bool compatableWithEachOther)
    {
        Player[] choices = EnumerateCompatiblePlayers(player, all).ToArray();
        if (choices.Length < amount)
        {
            return null;
        }

        if (!compatableWithEachOther)
        {
            return RandomHelper.EnumerateUniqueItems(_random, choices, amount);
        }

        List<IReadOnlyList<Player>> groups = EnumerateIntercompatibleGroups(choices, amount).ToList();
        return RandomHelper.SelectItem(_random, groups);

    }

    private readonly Random _random;
}