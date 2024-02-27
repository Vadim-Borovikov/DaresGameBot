using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Helpers;
using DaresGameBot.Game.Data.Players;

namespace DaresGameBot.Game.Matchmaking;

internal abstract class Matchmaker
{
    protected Matchmaker(Compatibility compatibility) => _compatibility = compatibility;

    public abstract IEnumerable<Player>? EnumerateMatches(Player player, IEnumerable<Player> all, byte amount,
        bool compatableWithEachOther);

    protected IEnumerable<Player> EnumerateCompatiblePlayers(Player player, IEnumerable<Player> all)
    {
        return all.Where(p => _compatibility.Check(p, player));
    }

    protected IEnumerable<IReadOnlyList<Player>> EnumerateIntercompatibleGroups(IList<Player> choices, byte size)
    {
        return ListHelper.EnumerateSubsets(choices, size).Select(s => s.AsReadOnly()).Where(_compatibility.Check);
    }

    private readonly Compatibility _compatibility;
}