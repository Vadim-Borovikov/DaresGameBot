using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Helpers;
using DaresGameBot.Game.Data.Players;

namespace DaresGameBot.Game.Matchmaking;

internal sealed class RandomMatchmaker : Matchmaker
{
    public RandomMatchmaker(Compatibility compatibility) : base(compatibility) { }

    public override IEnumerable<Player>? EnumerateMatches(Player player, IEnumerable<Player> all, byte amount,
        bool compatableWithEachOther)
    {
        Player[] choices = EnumerateCompatiblePlayers(player, all).ToArray();
        if (choices.Length < amount)
        {
            return null;
        }

        if (!compatableWithEachOther)
        {
            return RandomHelper.EnumerateUniqueItems(choices, amount);
        }

        List<IReadOnlyList<Player>> groups = EnumerateIntercompatibleGroups(choices, amount).ToList();
        return RandomHelper.SelectItem(groups);

    }
}