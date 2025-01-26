using DaresGameBot.Game.Data;
using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Game.Data.Cards;
using GryphonUtilities.Extensions;
using DaresGameBot.Helpers;

namespace DaresGameBot.Game.Matchmaking.ActionCheck;

internal sealed class CompanionsSelector : IActionChecker
{
    public CompanionsSelector(Matchmaker matchmaker, IReadOnlyList<string> players)
    {
        _matchmaker = matchmaker;
        _players = players;
    }

    public bool CanPlay(string player, Arrangement arrangement)
    {
        if ((arrangement.Partners + arrangement.Helpers) >= _players.Count)
        {
            return false;
        }

        return (arrangement.Partners == 0)
               || _matchmaker.AreThereAnyMatches(player, _players, arrangement.Partners,
                   arrangement.CompatablePartners);
    }

    public ArrangementInfo SelectCompanionsFor(string player, Arrangement arrangement)
    {
        List<string> partners = new();
        if (arrangement.Partners > 0)
        {
            partners = _matchmaker.EnumerateMatches(player, _players, arrangement.Partners, arrangement.CompatablePartners)
                                  .Denull("No suitable partners found")
                                  .ToList();
        }

        List<string> helpers = new();
        if (arrangement.Helpers > 0)
        {
            List<string> choices = _players.Where(p => (p != player) && !partners.Contains(p)).ToList();
            helpers = RandomHelper.EnumerateUniqueItems(choices, arrangement.Helpers).
                                   Denull("No suitable helpers found")
                                   .ToList();
        }

        return new ArrangementInfo(arrangement.GetHashCode(), player, partners, helpers);
    }

    private readonly IReadOnlyList<string> _players;
    private readonly Matchmaker _matchmaker;
}