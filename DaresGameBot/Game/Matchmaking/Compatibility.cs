using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Helpers;
using DaresGameBot.Game.Data.Players;

namespace DaresGameBot.Game.Matchmaking;

internal sealed class Compatibility
{
    public Compatibility(Dictionary<string, IInteractabilityProvider> playerInfos) => _playerInfos = playerInfos;

    public bool Check(Player p1, Player p2)
    {
        if (p1.Name == p2.Name)
        {
            return false;
        }

        IInteractabilityProvider info1 = _playerInfos[p1.Name];
        IInteractabilityProvider info2 = _playerInfos[p2.Name];

        return info1.WouldInteractWith(info2) && info2.WouldInteractWith(info1);
    }

    public bool Check(IReadOnlyList<Player> players) => ListHelper.EnumeratePairs(players).All(Check);

    private bool Check((Player, Player) pair) => Check(pair.Item1, pair.Item2);

    private readonly Dictionary<string, IInteractabilityProvider> _playerInfos;
}