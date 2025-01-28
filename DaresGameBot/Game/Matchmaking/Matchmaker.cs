using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Helpers;
using DaresGameBot.Game.Matchmaking.PlayerCheck;
using DaresGameBot.Game.Matchmaking.Interactions;

namespace DaresGameBot.Game.Matchmaking;

internal abstract class Matchmaker : IInteractionSubscriber
{
    protected Matchmaker(ICompatibility compatibility) => _compatibility = compatibility;

    public abstract void OnInteractionPurposed(string player, Arrangement arrangement);
    public abstract void OnInteractionCompleted(string player, Arrangement arrangement, string tag);

    public bool AreThereAnyMatches(string player, IEnumerable<string> all, byte amount, bool compatableWithEachOther)
    {
        List<string> choices = EnumerateCompatablePlayers(player, all).ToList();
        if (choices.Count < amount)
        {
            return false;
        }

        return !compatableWithEachOther || EnumerateIntercompatableGroups(choices, amount).Any();
    }

    public abstract IEnumerable<string>? EnumerateMatches(string player, IEnumerable<string> all, byte amount,
        bool compatableWithEachOther);

    protected IEnumerable<string> EnumerateCompatablePlayers(string player, IEnumerable<string> all)
    {
        return all.Where(p => _compatibility.AreCompatable(p, player));
    }

    protected IEnumerable<IReadOnlyList<string>> EnumerateIntercompatableGroups(IList<string> choices, byte size)
    {
        return ListHelper.EnumerateSubsets(choices, size)
                         .Select(s => s.AsReadOnly())
                         .Where(_compatibility.AreCompatable);
    }

    private readonly ICompatibility _compatibility;
}