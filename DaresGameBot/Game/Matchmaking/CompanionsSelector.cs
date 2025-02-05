using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Game.Data;
using GryphonUtilities.Extensions;

namespace DaresGameBot.Game.Matchmaking;

internal sealed class CompanionsSelector
{
    public CompanionsSelector(Matchmaker matchmaker, IReadOnlyList<string> players)
    {
        _matchmaker = matchmaker;
        _players = players;
    }

    public bool CanPlay(ArrangementType arrangementType)
    {
        if (arrangementType.Partners >= _players.Count)
        {
            return false;
        }

        return (arrangementType.Partners == 0) || _matchmaker.AreThereAnyMatches(arrangementType);
    }

    public Arrangement SelectCompanionsFor(ArrangementType arrangementType)
    {
        List<string> partners = new();
        if (arrangementType.Partners > 0)
        {
            partners = _matchmaker.EnumerateMatches(arrangementType)
                                  .Denull("No suitable partners found")
                                  .ToList();
        }
        return new Arrangement(partners, arrangementType.CompatablePartners);
    }

    private readonly IReadOnlyList<string> _players;
    private readonly Matchmaker _matchmaker;
}