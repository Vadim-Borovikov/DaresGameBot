using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Game.Data;
using GryphonUtilities.Extensions;

namespace DaresGameBot.Game.Matchmaking.ActionCheck;

internal sealed class CompanionsSelector : IActionChecker
{
    public CompanionsSelector(Matchmaker matchmaker, IReadOnlyList<string> players)
    {
        _matchmaker = matchmaker;
        _players = players;
    }

    public bool CanPlay(string player, ArrangementType arrangementType)
    {
        if (arrangementType.Partners >= _players.Count)
        {
            return false;
        }

        return (arrangementType.Partners == 0) || _matchmaker.AreThereAnyMatches(player, _players, arrangementType);
    }

    public Arrangement SelectCompanionsFor(string player, ArrangementType arrangementType)
    {
        List<string> partners = new();
        if (arrangementType.Partners > 0)
        {
            partners = _matchmaker.EnumerateMatches(player, _players, arrangementType)
                                  .Denull("No suitable partners found")
                                  .ToList();
        }
        return new Arrangement(partners, arrangementType.CompatablePartners);
    }

    private readonly IReadOnlyList<string> _players;
    private readonly Matchmaker _matchmaker;
}