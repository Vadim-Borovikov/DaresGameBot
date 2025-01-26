using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Helpers;
using DaresGameBot.Game.Matchmaking.PlayerCheck;

namespace DaresGameBot.Game.Matchmaking;

internal abstract class Matchmaker
{
    protected Matchmaker(ICompatibility compatibility) => _compatibility = compatibility;

    public bool AreThereAnyMatches(string player, IEnumerable<string> all, byte amount, bool compatableWithEachOther)
    {
        List<string> choices = EnumerateCompatiblePlayers(player, all).ToList();
        if (choices.Count < amount)
        {
            return false;
        }

        return !compatableWithEachOther || EnumerateIntercompatibleGroups(choices, amount).Any();
    }

    public abstract IEnumerable<string>? EnumerateMatches(string player, IEnumerable<string> all, byte amount,
        bool compatableWithEachOther);

    protected IEnumerable<string> EnumerateCompatiblePlayers(string player, IEnumerable<string> all)
    {
        return all.Where(p => _compatibility.AreCompatable(p, player));
    }

    protected IEnumerable<IReadOnlyList<string>> EnumerateIntercompatibleGroups(IList<string> choices, byte size)
    {
        return ListHelper.EnumerateSubsets(choices, size)
                         .Select(s => s.AsReadOnly())
                         .Where(_compatibility.AreCompatable);
    }

    private readonly ICompatibility _compatibility;
}