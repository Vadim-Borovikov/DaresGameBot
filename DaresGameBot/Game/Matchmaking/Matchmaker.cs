using DaresGameBot.Game.Data;
using DaresGameBot.Game.Matchmaking.Compatibility;
using DaresGameBot.Game.States;
using DaresGameBot.Utilities;
using GryphonUtilities.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace DaresGameBot.Game.Matchmaking;

internal abstract class Matchmaker
{
    protected Matchmaker(PlayersRepository players, ICompatibility compatibility)
    {
        Players = players;
        _compatibility = compatibility;
    }

    public bool CanPlay(ArrangementType arrangementType)
    {
        if (arrangementType.Partners >= Players.GetActiveIds().Count())
        {
            return false;
        }

        return (arrangementType.Partners == 0) || AreThereAnyMatches(arrangementType);
    }

    public bool CanBePlayed(Arrangement arrangement)
    {
        List<long> compatable = EnumerateCompatablePlayers().ToList();

        return arrangement.Partners.All(compatable.Contains)
               && (!arrangement.CompatablePartners || Players.IsIntercompatable(arrangement.Partners, _compatibility));
    }

    public Arrangement SelectCompanionsFor(ArrangementType arrangementType)
    {
        List<long> partners = new();
        if (arrangementType.Partners > 0)
        {
            partners = EnumerateMatches(arrangementType).Denull("No suitable partners found").ToList();
        }
        return new Arrangement(partners, arrangementType.CompatablePartners);
    }

    protected IEnumerable<long> EnumerateCompatablePlayers()
    {
        return Players.GetActiveIds().Where(p => Players.AreCompatable(p, Players.Current, _compatibility));
    }

    protected IEnumerable<IReadOnlyList<long>> EnumerateIntercompatableGroups(IList<long> choices, byte size)
    {
        if (choices.Count < size)
        {
            yield break;
        }

        if (choices.Count == size)
        {
            IReadOnlyList<long> choisesAsReadOnly = choices.AsReadOnly();
            if (Players.IsIntercompatable(choisesAsReadOnly, _compatibility))
            {
                yield return choisesAsReadOnly;
            }
            yield break;
        }

        foreach (IList<long> subset in
                 ListHelper.EnumerateStrictSubsets(choices, size,
                     (p, g) => Players.IsCompatableWith(p, g, _compatibility)))
        {
            yield return subset.AsReadOnly();
        }
    }

    private bool AreThereAnyMatches(ArrangementType arrangementType)
    {
        List<long> choices = EnumerateCompatablePlayers().ToList();
        if (choices.Count < arrangementType.Partners)
        {
            return false;
        }

        return !arrangementType.CompatablePartners
               || EnumerateIntercompatableGroups(choices, arrangementType.Partners).Any();
    }

    protected abstract IEnumerable<long>? EnumerateMatches(ArrangementType arrangementType);

    protected readonly PlayersRepository Players;

    private readonly ICompatibility _compatibility;
}