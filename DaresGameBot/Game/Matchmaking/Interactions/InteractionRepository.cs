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

    public int GetInteractions(string player, IReadOnlyList<string> players, bool completed)
    {
        int result = 0;

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

    public int GetInteractions(string p1, string p2, bool completed)
    {
        Dictionary<string, int> repository = completed ? _interactionsCompleted : _interactionsPurposed;
        string key = GetKey(p1, p2);
        return repository.GetValueOrDefault(key);
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
        Dictionary<string, int> repository = points is null ? _interactionsPurposed : _interactionsCompleted;
        points ??= 1;

        string key = GetKey(p1, p2);
        repository.CreateOrAdd(key, points.Value);
    }

    private static string GetKey(string p1, string p2)
    {
        return string.Compare(p1, p2, StringComparison.Ordinal) < 0 ? p1 + p2 : p2 + p1;
    }

    private readonly PointsManager _pointsManager;
    private readonly Dictionary<string, int> _interactionsPurposed = new();
    private readonly Dictionary<string, int> _interactionsCompleted = new();
}