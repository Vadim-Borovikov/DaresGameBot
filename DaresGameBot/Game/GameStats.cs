using System;
using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Configs;
using DaresGameBot.Game.Matchmaking.Interactions;
using DaresGameBot.Game.Players;
using DaresGameBot.Helpers;
using DaresGameBot.Operations.Data.PlayerListUpdates;

namespace DaresGameBot.Game;

internal sealed class GameStats : IInteractionSubscriber
{
    public GameStats(Dictionary<string, Option> actionOptions, Repository players)
    {
        _actionOptions = actionOptions;
        _players = players;
    }

    public void OnInteractionPurposed(string player, Arrangement arrangement)
    {
        RegisterInteractions(player, arrangement);
    }

    public void OnInteractionCompleted(string player, Arrangement arrangement, string tag, bool completedFully)
    {
        int? points = GetPoints(tag, completedFully);
        if (points is null)
        {
            throw new NullReferenceException($"No points in config for ({tag}, {completedFully})");
        }

        RegisterInteractions(player, arrangement, points);

        _points.CreateOrAdd(player, points.Value);
        foreach (string partner in arrangement.Partners)
        {
            _points.CreateOrAdd(partner, points.Value);
        }
    }

    public int GetPoints(string name) => _points[name];

    public bool UpdateList(List<PlayerListUpdateData> updateDatas)
    {
        int minPoints = _points.Any() ? _points.Values.Min() : 0;
        bool changed = false;

        foreach (PlayerListUpdateData data in updateDatas)
        {
            switch (data)
            {
                case AddOrUpdatePlayerData a:
                    if (!_players.GetNames().Contains(a.Name))
                    {
                        EnsureMinPoints(a.Name, minPoints);
                    }

                    changed |= _players.AddOrUpdatePlayerData(a);

                    break;
                case TogglePlayerData t:
                    if (_players.TogglePlayerData(t))
                    {
                        if (_players.GetNames().Contains(t.Name))
                        {
                            EnsureMinPoints(t.Name, minPoints);
                        }

                        changed = true;
                    }

                    break;
            }
        }

        return changed;
    }

    public int GetInteractions(string player, IReadOnlyList<string> players, bool completed)
    {
        return players.Sum(p => GetInteractions(player, p, completed))
               + ListHelper.EnumeratePairs(players)
                           .Sum(pair => GetInteractions(pair.Item1, pair.Item2, completed));
    }

    public int GetInteractions(string p1, string p2, bool completed)
    {
        Dictionary<string, int> repository = completed ? _interactionsCompleted : _interactionsPurposed;
        string key = GetKey(p1, p2);
        return repository.GetValueOrDefault(key);
    }

    private int? GetPoints(string tag, bool completedFully)
    {
        if (!_actionOptions.ContainsKey(tag))
        {
            return null;
        }
        return completedFully ? _actionOptions[tag].Points : _actionOptions[tag].PartialPoints;
    }

    private void EnsureMinPoints(string name, int points)
    {
        _points[name] = Math.Max(_points.GetValueOrDefault(name), points);
    }

    private void RegisterInteractions(string player, Arrangement arrangement, int? points = null)
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

    private void RegisterInteraction(string p1, string p2, int? points = null)
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

    private readonly Dictionary<string, Option> _actionOptions;
    private readonly Repository _players;
    private readonly Dictionary<string, int> _points = new();
    private readonly Dictionary<string, int> _interactionsPurposed = new();
    private readonly Dictionary<string, int> _interactionsCompleted = new();
}