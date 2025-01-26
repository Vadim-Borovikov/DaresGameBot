using System.Collections.Generic;
using DaresGameBot.Helpers;

namespace DaresGameBot.Game.Matchmaking.Interactions;

internal sealed class InteractionRepository : IInteractionSubscriber
{
    public void OnInteraction(string player, IReadOnlyList<string> partners, bool actionsBetweenPartners, ushort _)
    {
        foreach (string p in partners)
        {
            RegisterInteraction(player, p);
        }

        if (!actionsBetweenPartners)
        {
            return;
        }
        foreach ((string, string) pair in ListHelper.EnumeratePairs(partners))
        {
            RegisterInteraction(pair.Item1, pair.Item2);
        }
    }

    public ushort GetInteractions(string player, IReadOnlyList<string> players)
    {
        ushort result = 0;

        foreach (string p in players)
        {
            result += GetInteractions(player, p);
        }

        foreach ((string, string) pair in ListHelper.EnumeratePairs(players))
        {
            result += GetInteractions(pair.Item1, pair.Item2);
        }

        return result;
    }

    public ushort GetInteractions(string p1, string p2)
    {
        int hash = GetHash(p1, p2);
        return _interactions.GetValueOrDefault(hash);
    }

    private void RegisterInteraction(string p1, string p2)
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

    private static int GetHash(string p1, string p2) => p1.GetHashCode() ^ p2.GetHashCode();

    private readonly Dictionary<int, ushort> _interactions = new();
}