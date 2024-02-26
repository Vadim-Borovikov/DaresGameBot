using DaresGameBot.Game.Data;
using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Helpers;

namespace DaresGameBot.Game.Matchmaking;

internal class Compatibility
{
    public virtual bool Check(Player p1, Player p2) => p1.Name != p2.Name;

    public bool Check(IReadOnlyList<Player> players) => ListHelper.EnumeratePairs(players).All(Check);

    private bool Check((Player, Player) pair) => Check(pair.Item1, pair.Item2);
}