using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Helpers;

namespace DaresGameBot.Game.Matchmaking.PlayerCheck;

internal sealed class Compatibility : Dictionary<string, GroupChecker>
{
    public bool AreCompatable(string p1, string p2)
    {
        if (p1 == p2)
        {
            return false;
        }

        GroupChecker info1 = this[p1];
        GroupChecker info2 = this[p2];

        return info1.WouldInteractWith(info2) && info2.WouldInteractWith(info1);
    }

    public bool AreCompatable(IReadOnlyList<string> players) => ListHelper.EnumeratePairs(players).All(AreCompatable);

    private bool AreCompatable((string, string) pair) => AreCompatable(pair.Item1, pair.Item2);
}