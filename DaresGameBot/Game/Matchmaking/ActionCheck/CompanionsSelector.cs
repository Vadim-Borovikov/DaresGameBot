using DaresGameBot.Game.Data;
using DaresGameBot.Game.Data.Cards;
using DaresGameBot.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace DaresGameBot.Game.Matchmaking.ActionCheck;

internal sealed class CompanionsSelector : IActionChecker
{
    public CompanionsSelector(Matchmaker matchmaker, IReadOnlyList<string> players)
    {
        _matchmaker = matchmaker;
        _players = players;
    }

    public bool CanPlay(string player, CardAction action)
    {
        if ((action.Partners + action.Helpers) >= _players.Count)
        {
            return false;
        }

        return (action.Partners == 0)
               || _matchmaker.AreThereAnyMatches(player, _players, action.Partners, action.CompatablePartners);
    }

    public CompanionsInfo? TrySelectCompanionsFor(string player, CardAction action)
    {
        List<string>? partners = null;
        if (action.Partners > 0)
        {
            partners =
                _matchmaker.EnumerateMatches(player, _players, action.Partners, action.CompatablePartners)?.ToList();
            if (partners is null)
            {
                return null;
            }
        }

        List<string>? helpers = null;
        if (action.Helpers > 0)
        {
            List<string> choices =
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

    private readonly IReadOnlyList<string> _players;
    private readonly Matchmaker _matchmaker;
}