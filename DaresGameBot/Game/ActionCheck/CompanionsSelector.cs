using System;
using DaresGameBot.Game.Data;
using DaresGameBot.Game.Data.Cards;
using DaresGameBot.Game.Matchmaking;
using DaresGameBot.Helpers;
using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Game.Data.Players;

namespace DaresGameBot.Game.ActionCheck;

internal sealed class CompanionsSelector : IActionChecker
{
    public Matchmaker Matchmaker;

    public CompanionsSelector(Matchmaker matchmaker, IReadOnlyList<Player> players)
    {
        Matchmaker = matchmaker;
        _players = players;
        _partnersCache = new Dictionary<int, List<Player>?>();
    }

    public bool Check(Player player, CardAction action)
    {
        List<Player>? partners = TryGetPartnersFromCache(player, action);
        if (partners is not null)
        {
            return true;
        }

        if ((action.Partners + action.Helpers) >= _players.Count)
        {
            return false;
        }

        if (action.Partners == 0)
        {
            return true;
        }

        partners = Matchmaker.EnumerateMatches(player, _players, action.Partners, action.CompatablePartners)?.ToList();
        if (partners is null)
        {
            return false;
        }
        AddPartnersToCache(player, action, partners);
        return true;
    }

    public CompanionsInfo? TrySelectCompanionsFor(Player player, CardAction action)
    {
        List<Player>? partners = null;
        if (action.Partners > 0)
        {
            partners =
                TryGetPartnersFromCache(player, action)
                ?? Matchmaker.EnumerateMatches(player, _players, action.Partners, action.CompatablePartners)?.ToList();
            if (partners is null)
            {
                return null;
            }
        }

        List<Player>? helpers = null;
        if (action.Helpers > 0)
        {
            List<Player> choices =
                _players.Where(p => (p != player) && (partners is null || !partners.Contains(p))).ToList();
            helpers = RandomHelper.EnumerateUniqueItems(choices, action.Helpers)?.ToList();
            if (helpers is null)
            {
                return null;
            }
        }

        if (!action.AssignPartners)
        {
            partners = null;
        }

        return new CompanionsInfo(player, partners, helpers);
    }

    private static int GetHachCode(Player player, CardAction action) => HashCode.Combine(player.Name, action.Id);

    private void AddPartnersToCache(Player player, CardAction action, List<Player> partners)
    {
        int hash = GetHachCode(player, action);
        _partnersCache[hash] = partners;
    }

    private List<Player>? TryGetPartnersFromCache(Player player, CardAction card)
    {
        int hash = GetHachCode(player, card);
        return _partnersCache.GetValueOrDefault(hash);
    }

    private readonly IReadOnlyList<Player> _players;
    private readonly Dictionary<int, List<Player>?> _partnersCache;
}