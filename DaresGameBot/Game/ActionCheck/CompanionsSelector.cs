using DaresGameBot.Game.Data;
using DaresGameBot.Game.Data.Cards;
using DaresGameBot.Game.Data.Players;
using DaresGameBot.Game.Matchmaking;
using DaresGameBot.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace DaresGameBot.Game.ActionCheck;

internal sealed class CompanionsSelector : IActionChecker
{
    public DistributedMatchmaker Matchmaker;

    public CompanionsSelector(DistributedMatchmaker matchmaker, IReadOnlyList<Player> players)
    {
        Matchmaker = matchmaker;
        _players = players;
    }

    public bool Check(Player player, CardAction action)
    {
        if ((action.Partners + action.Helpers) >= _players.Count)
        {
            return false;
        }

        return (action.Partners == 0)
               || Matchmaker.AreThereAnyMatches(player, _players, action.Partners, action.CompatablePartners);
    }

    public CompanionsInfo? TrySelectCompanionsFor(Player player, CardAction action)
    {
        List<Player>? partners = null;
        if (action.Partners > 0)
        {
            partners =
                Matchmaker.EnumerateMatches(player, _players, action.Partners, action.CompatablePartners)?.ToList();
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

    private readonly IReadOnlyList<Player> _players;
}