using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Helpers;

namespace DaresGameBot.Game.Matchmaking.PlayerCheck;

internal sealed class Compatibility
{
    public Compatibility(Dictionary<string, IPartnerChecker>? playerInfos = null)
    {
        _playerInfos = playerInfos ?? new Dictionary<string, IPartnerChecker>();
    }

    public void AddPlayer(string player, IPartnerChecker checker) => _playerInfos.Add(player, checker);

    public bool AreCompatable(string p1, string p2)
    {
        if (p1 == p2)
        {
            return false;
        }

        IPartnerChecker info1 = _playerInfos[p1];
        IPartnerChecker info2 = _playerInfos[p2];

        return info1.WouldInteractWith(info2) && info2.WouldInteractWith(info1);
    }

    public bool AreCompatable(IReadOnlyList<string> players) => ListHelper.EnumeratePairs(players).All(AreCompatable);

    private bool AreCompatable((string, string) pair) => AreCompatable(pair.Item1, pair.Item2);

    private readonly Dictionary<string, IPartnerChecker> _playerInfos;
}