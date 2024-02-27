using DaresGameBot.Game.Data;
using DaresGameBot.Game.Data.Cards;
using DaresGameBot.Game.Matchmaking;
using DaresGameBot.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace DaresGameBot.Game.ActionCheck;

internal sealed class CompanionsSelector : IActionChecker
{
    public Matchmaker Matchmaker;

    public CompanionsInfo? CompanionsInfo { get; private set; }

    private readonly IReadOnlyList<Player> _players;

    public CompanionsSelector(Matchmaker matchmaker, IReadOnlyList<Player> players)
    {
        Matchmaker = matchmaker;
        _players = players;
    }

    public bool Check(Player player, CardAction card)
    {
        CompanionsInfo = TryGetCompanionsFor(player, card);
        return CompanionsInfo is not null;
    }

    private CompanionsInfo? TryGetCompanionsFor(Player player, CardAction card)
    {
        List<Player>? partners = null;
        if (card.Partners > 0)
        {
            partners = Matchmaker.EnumerateMatches(player, _players, card.Partners, card.CompatablePartners)?.ToList();
            if (partners is null)
            {
                return null;
            }
        }

        List<Player>? helpers = null;
        if (card.Helpers > 0)
        {
            List<Player> choices =
                _players.Where(p => (p != player) && (partners is null || !partners.Contains(p))).ToList();
            helpers = RandomHelper.EnumerateUniqueItems(choices, card.Helpers)?.ToList();
            if (helpers is null)
            {
                return null;
            }
        }

        if (!card.AssignPartners)
        {
            partners = null;
        }

        return new CompanionsInfo(player, partners, helpers);
    }
}