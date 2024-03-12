using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Helpers;
using DaresGameBot.Game.Data.Players;

namespace DaresGameBot.Game.Matchmaking.PlayerCheck;

internal sealed class Compatibility
{
    public Compatibility(Dictionary<string, IPartnerChecker> playerInfos) => _playerInfos = playerInfos;

    public bool AreCompatable(Player p1, Player p2)
    {
        if (p1.Name == p2.Name)
        {
            return false;
        }

        IPartnerChecker info1 = _playerInfos[p1.Name];
        IPartnerChecker info2 = _playerInfos[p2.Name];

        return info1.WouldInteractWith(info2) && info2.WouldInteractWith(info1);
    }

    public bool AreCompatable(IReadOnlyList<Player> players) => ListHelper.EnumeratePairs(players).All(AreCompatable);

    private bool AreCompatable((Player, Player) pair) => AreCompatable(pair.Item1, pair.Item2);

    private readonly Dictionary<string, IPartnerChecker> _playerInfos;
}