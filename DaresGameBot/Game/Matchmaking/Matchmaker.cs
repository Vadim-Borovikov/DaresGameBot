using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Game.Data;
using DaresGameBot.Helpers;
using DaresGameBot.Game.Matchmaking.Interactions;

namespace DaresGameBot.Game.Matchmaking;

internal abstract class Matchmaker : IInteractionSubscriber
{
    protected Matchmaker(Players.Repository players) => Players = players;

    public abstract void OnInteractionPurposed(string player, Arrangement arrangement);
    public abstract void OnInteractionCompleted(string player, Arrangement arrangement, string tag,
        bool completedFully);

    public bool AreThereAnyMatches(ArrangementType arrangementType)
    {
        List<string> choices = EnumerateCompatablePlayers().ToList();
        if (choices.Count < arrangementType.Partners)
        {
            return false;
        }

        return !arrangementType.CompatablePartners
               || EnumerateIntercompatableGroups(choices, arrangementType.Partners).Any();
    }

    public abstract IEnumerable<string>? EnumerateMatches(ArrangementType arrangementType);

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

    protected readonly Players.Repository Players;
}