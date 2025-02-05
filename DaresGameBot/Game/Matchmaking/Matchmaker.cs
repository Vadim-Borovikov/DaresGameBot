using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Game.Data;
using DaresGameBot.Helpers;
using DaresGameBot.Game.Matchmaking.Interactions;
using GryphonUtilities.Extensions;

namespace DaresGameBot.Game.Matchmaking;

internal abstract class Matchmaker : IInteractionSubscriber
{
    protected Matchmaker(Players.Repository players) => Players = players;

    public abstract void OnInteractionPurposed(string player, Arrangement arrangement);
    public abstract void OnInteractionCompleted(string player, Arrangement arrangement, string tag,
        bool completedFully);

    public bool CanPlay(ArrangementType arrangementType)
    {
        if (arrangementType.Partners >= Players.GetNames().Count)
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
        return Players.GetNames().Where(p => Players.AreCompatable(p, Players.Current));
    }

    protected IEnumerable<IReadOnlyList<string>> EnumerateIntercompatableGroups(IList<string> choices, byte size)
    {
        return ListHelper.EnumerateSubsets(choices, size)
                         .Select(s => s.AsReadOnly())
                         .Where(Players.AreCompatable);
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
}