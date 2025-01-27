using DaresGameBot.Game.Data;
using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Game.Data.Cards;
using GryphonUtilities.Extensions;

namespace DaresGameBot.Game.Matchmaking.ActionCheck;

internal sealed class CompanionsSelector : IActionChecker
{
    public CompanionsSelector(Matchmaker matchmaker, IReadOnlyList<string> players)
    {
        _matchmaker = matchmaker;
        _players = players;
    }

    public bool CanPlay(string player, Arrangement arrangement, byte helpers)
    {
        if ((arrangement.Partners + helpers) >= _players.Count)
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
        return new ArrangementInfo(arrangement.GetHashCode(), partners);
    }

    private readonly IReadOnlyList<string> _players;
    private readonly Matchmaker _matchmaker;
}