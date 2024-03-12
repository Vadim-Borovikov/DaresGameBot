using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Helpers;
using DaresGameBot.Game.Data.Players;
using DaresGameBot.Game.Matchmaking.PlayerCheck;

namespace DaresGameBot.Game.Matchmaking;

internal abstract class Matchmaker
{
    public Compatibility Compatibility { private get; set; }

    protected Matchmaker(Compatibility compatibility) => Compatibility = compatibility;

    public bool AreThereAnyMatches(Player player, IEnumerable<Player> all, byte amount,
        bool compatableWithEachOther)
    {
        List<Player> choices = EnumerateCompatiblePlayers(player, all).ToList();
        if (choices.Count < amount)
        {
            return false;
        }

        return !compatableWithEachOther || EnumerateIntercompatibleGroups(choices, amount).Any();
    }

    public abstract IEnumerable<Player>? EnumerateMatches(Player player, IEnumerable<Player> all, byte amount,
        bool compatableWithEachOther);

    protected IEnumerable<Player> EnumerateCompatiblePlayers(Player player, IEnumerable<Player> all)
    {
        return all.Where(p => Compatibility.AreCompatable(p, player));
    }

    protected IEnumerable<IReadOnlyList<Player>> EnumerateIntercompatibleGroups(IList<Player> choices, byte size)
    {
        return ListHelper.EnumerateSubsets(choices, size)
                         .Select(s => s.AsReadOnly())
                         .Where(Compatibility.AreCompatable);
    }
}