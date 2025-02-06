using System;
using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Configs;
using DaresGameBot.Game.Matchmaking.Interactions;
using DaresGameBot.Game.Players;
using DaresGameBot.Helpers;
using DaresGameBot.Operations.Data.PlayerListUpdates;

namespace DaresGameBot.Game;

internal sealed class PointsManager : IInteractionSubscriber
{
    public PointsManager(Dictionary<string, Option> actionOptions, Repository players)
    {
        _actionOptions = actionOptions;
        _players = players;
    }

    public int? GetPoints(string tag, bool completedFully)
    {
        if (!_actionOptions.ContainsKey(tag))
        {
            return null;
        }
        return completedFully ? _actionOptions[tag].Points : _actionOptions[tag].PartialPoints;
    }

    public void OnInteractionCompleted(string player, Arrangement arrangement, string tag, bool completedFully)
    {
        int? points = GetPoints(tag, completedFully);
        if (points is null)
        {
            throw new NullReferenceException($"No points in config for ({tag}, {completedFully})");
        }

        AddPoints(player, points.Value);
        foreach (string partner in arrangement.Partners)
        {
            AddPoints(partner, points.Value);
        }
    }

    public bool UpdateList(List<PlayerListUpdateData> updateDatas)
    {
        int minPoints = _points.Values.Min();
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

    public int GetPoints(string name) => _points[name];

    private void AddPoints(string name, int points) => _points.CreateOrAdd(name, points);

    private void EnsureMinPoints(string name, int points)
    {
        _points[name] = Math.Max(_points.GetValueOrDefault(name), points);
    }

    private readonly Dictionary<string, Option> _actionOptions;
    private readonly Repository _players;
    private readonly Dictionary<string, int> _points = new();
}