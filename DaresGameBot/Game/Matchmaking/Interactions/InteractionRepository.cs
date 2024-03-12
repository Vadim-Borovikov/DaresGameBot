using System.Collections.Generic;
using DaresGameBot.Helpers;
using DaresGameBot.Game.Data.Players;

namespace DaresGameBot.Game.Matchmaking.Interactions;

internal sealed class InteractionRepository : IInteractionSubscriber
{
    public void OnInteraction(Player player, IReadOnlyList<Player> partners, bool actionsBetweenPartners)
    {
        foreach (Player p in partners)
        {
            RegisterInteraction(player, p);
        }

        if (!actionsBetweenPartners)
        {
            return;
        }
        foreach ((Player, Player) pair in ListHelper.EnumeratePairs(partners))
        {
            RegisterInteraction(pair.Item1, pair.Item2);
        }
    }

    public ushort GetInteractions(Player player, IReadOnlyList<Player> players)
    {
        ushort result = 0;

        foreach (Player p in players)
        {
            result += GetInteractions(player, p);
        }

        foreach ((Player, Player) pair in ListHelper.EnumeratePairs(players))
        {
            result += GetInteractions(pair.Item1, pair.Item2);
        }

        return result;
    }

    public ushort GetInteractions(Player p1, Player p2)
    {
        int hash = GetHash(p1, p2);
        return _interactions.GetValueOrDefault(hash);
    }

    private void RegisterInteraction(Player p1, Player p2)
    {
        int hash = GetHash(p1, p2);
        if (_interactions.ContainsKey(hash))
        {
            ++_interactions[hash];
        }
        else
        {
            _interactions[hash] = 1;
        }
    }

    private static int GetHash(Player p1, Player p2) => p1.Name.GetHashCode() ^ p2.Name.GetHashCode();

    private readonly Dictionary<int, ushort> _interactions = new();
}