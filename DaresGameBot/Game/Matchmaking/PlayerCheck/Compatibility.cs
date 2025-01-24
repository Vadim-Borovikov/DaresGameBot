using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Helpers;

namespace DaresGameBot.Game.Matchmaking.PlayerCheck;

internal sealed class Compatibility
{
    public readonly Dictionary<string, IPartnerChecker> PlayerInfos;

    public Compatibility(Dictionary<string, IPartnerChecker>? playerInfos = null)
    {
        PlayerInfos = playerInfos ?? new Dictionary<string, IPartnerChecker>();
    }

    public bool AreCompatable(string p1, string p2)
    {
        if (p1 == p2)
        {
            return false;
        }

        IPartnerChecker info1 = PlayerInfos[p1];
        IPartnerChecker info2 = PlayerInfos[p2];

        return info1.WouldInteractWith(info2) && info2.WouldInteractWith(info1);
    }

    public bool AreCompatable(IReadOnlyList<string> players) => ListHelper.EnumeratePairs(players).All(AreCompatable);

    private bool AreCompatable((string, string) pair) => AreCompatable(pair.Item1, pair.Item2);
}