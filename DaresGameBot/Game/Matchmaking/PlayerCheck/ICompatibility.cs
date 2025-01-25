using DaresGameBot.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace DaresGameBot.Game.Matchmaking.PlayerCheck;

internal interface ICompatibility
{
    bool AreCompatable(string p1, string p2);

    public bool AreCompatable(IReadOnlyList<string> players) => ListHelper.EnumeratePairs(players).All(AreCompatable);

    private bool AreCompatable((string, string) pair) => AreCompatable(pair.Item1, pair.Item2);
}