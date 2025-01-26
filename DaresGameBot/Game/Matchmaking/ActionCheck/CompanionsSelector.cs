using DaresGameBot.Game.Data;
using System.Collections.Generic;
using System.Linq;
using GryphonUtilities.Extensions;

namespace DaresGameBot.Game.Matchmaking.ActionCheck;

internal sealed class CompanionsSelector : IActionChecker
{
    public CompanionsSelector(Matchmaker matchmaker, IReadOnlyList<string> players)
    {
        _matchmaker = matchmaker;
        _players = players;
    }

    public bool CanPlay(string player, Data.Cards.Action action)
    {
        if (action.Partners >= _players.Count)
        {
            return false;
        }

        return (action.Partners == 0)
               || _matchmaker.AreThereAnyMatches(player, _players, action.Partners, action.CompatablePartners);
    }

    public ActionInfo SelectCompanionsFor(string player, ushort actionId, Data.Cards.Action action)
    {
        List<string> partners = new();
        if (action.Partners > 0)
        {
            partners = _matchmaker.EnumerateMatches(player, _players, action.Partners, action.CompatablePartners)
                                  .Denull("No suitable partners found")
                                  .ToList();
        }
        return new ActionInfo(player, partners, actionId);
    }

    private readonly IReadOnlyList<string> _players;
    private readonly Matchmaker _matchmaker;
}