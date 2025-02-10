using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Game.Data;
using DaresGameBot.Game.Matchmaking.Compatibility;
using DaresGameBot.Helpers;
using GryphonUtilities.Extensions;

namespace DaresGameBot.Game.Matchmaking;

internal abstract class Matchmaker
{
    protected Matchmaker(Players.Repository players, ICompatibility compatibility)
    {
        Players = players;
        _compatibility = compatibility;
    }

    public bool CanPlay(ArrangementType arrangementType)
    {
        if (arrangementType.Partners >= Players.GetActiveNames().Count())
        {
            return false;
        }

        return (arrangementType.Partners == 0) || AreThereAnyMatches(arrangementType);
    }

    public Arrangement SelectCompanionsFor(ArrangementType arrangementType)
    {
        List<string> partners = new();
        if (arrangementType.Partners > 0)
        {
            partners = EnumerateMatches(arrangementType).Denull("No suitable partners found").ToList();
        }
        return new Arrangement(partners, arrangementType.CompatablePartners);
    }

    protected IEnumerable<string> EnumerateCompatablePlayers()
    {
        return Players.GetActiveNames().Where(p => Players.AreCompatable(p, Players.Current, _compatibility));
    }

    protected IEnumerable<IReadOnlyList<string>> EnumerateIntercompatableGroups(IList<string> choices, byte size)
    {
        return ListHelper.EnumerateSubsets(choices, size)
                         .Select(s => s.AsReadOnly())
                         .Where(g => Players.AreCompatable(g, _compatibility));
    }

    private bool AreThereAnyMatches(ArrangementType arrangementType)
    {
        List<string> choices = EnumerateCompatablePlayers().ToList();
        if (choices.Count < arrangementType.Partners)
        {
            return false;
        }

        return !arrangementType.CompatablePartners
               || EnumerateIntercompatableGroups(choices, arrangementType.Partners).Any();
    }

    protected abstract IEnumerable<string>? EnumerateMatches(ArrangementType arrangementType);

    protected readonly Players.Repository Players;

    private readonly ICompatibility _compatibility;
}