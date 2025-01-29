using System;
using System.Collections.Generic;
using DaresGameBot.Helpers;

namespace DaresGameBot.Game.Matchmaking.Interactions;

internal sealed class InteractionRepository : IInteractionSubscriber
{
    public InteractionRepository(PointsManager pointsManager) => _pointsManager = pointsManager;

    public void OnInteractionPurposed(string player, Arrangement arrangement)
    {
        RegisterInteractions(player, arrangement);
    }

    public void OnInteractionCompleted(string player, Arrangement arrangement, string tag, bool completedFully)
    {
        ushort? points = _pointsManager.GetPoints(tag, !completedFully);
        if (points is null)
        {
            throw new NullReferenceException($"No points in config for ({tag}, {completedFully})");
        }
        RegisterInteractions(player, arrangement, points);
    }

    public ushort GetInteractions(string player, IReadOnlyList<string> players, bool completed)
    {
        ushort result = 0;

        foreach (string p in players)
        {
            result += GetInteractions(player, p, completed);
        }

        foreach ((string, string) pair in ListHelper.EnumeratePairs(players))
        {
            result += GetInteractions(pair.Item1, pair.Item2, completed);
        }

        return result;
    }

    public ushort GetInteractions(string p1, string p2, bool completed)
    {
        Dictionary<int, ushort> repository = completed ? _interactionsCompleted : _interactionsPurposed;
        int hash = GetHash(p1, p2);
        return repository.GetValueOrDefault(hash);
    }

    private void RegisterInteractions(string player, Arrangement arrangement, ushort? points = null)
    {
        foreach (string p in arrangement.Partners)
        {
            RegisterInteraction(player, p, points);
        }

        if (!arrangement.CompatablePartners)
        {
            return;
        }
        foreach ((string, string) pair in ListHelper.EnumeratePairs(arrangement.Partners))
        {
            RegisterInteraction(pair.Item1, pair.Item2, points);
        }
    }

    private void RegisterInteraction(string p1, string p2, ushort? points = null)
    {
        Dictionary<int, ushort> repository = points is null ? _interactionsPurposed : _interactionsCompleted;
        points ??= 1;

        int hash = GetHash(p1, p2);
        if (repository.ContainsKey(hash))
        {
            repository[hash] += points.Value;
        }
        else
        {
            repository[hash] = points.Value;
        }
    }

    private static int GetHash(string p1, string p2) => p1.GetHashCode() ^ p2.GetHashCode();

    private readonly PointsManager _pointsManager;
    private readonly Dictionary<int, ushort> _interactionsPurposed = new();
    private readonly Dictionary<int, ushort> _interactionsCompleted = new();
}