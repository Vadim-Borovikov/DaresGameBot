using DaresGameBot.Game.Data;
using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Helpers;

namespace DaresGameBot.Game.Matchmaking;

internal class Matchmaker
{
    public virtual bool AreCompatable(Player p1, Player p2) => p1.Name != p2.Name;

    public bool AreCompatable(IReadOnlyList<Player> players) => ListHelper.EnumeratePairs(players).All(AreCompatable);

    private bool AreCompatable((Player, Player) pair) => AreCompatable(pair.Item1, pair.Item2);
}