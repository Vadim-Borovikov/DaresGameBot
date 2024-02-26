using DaresGameBot.Game.Data;
using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Helpers;

namespace DaresGameBot.Game.Matchmaking;

internal class Matchmaker
{
    public virtual bool AreCompatible(Player p1, Player p2) => p1.Name != p2.Name;

    public bool AreCompatible(IReadOnlyList<Player> players) => ListHelper.EnumeratePairs(players).All(AreCompatible);

    private bool AreCompatible((Player, Player) pair) => AreCompatible(pair.Item1, pair.Item2);
}