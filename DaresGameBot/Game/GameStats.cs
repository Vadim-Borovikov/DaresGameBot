using System;
using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Configs;
using DaresGameBot.Game.Data;
using DaresGameBot.Game.Matchmaking.Interactions;
using DaresGameBot.Helpers;
using DaresGameBot.Operations.Data.PlayerListUpdates;

namespace DaresGameBot.Game;

internal sealed class GameStats : IInteractionSubscriber
{
    public IReadOnlyDictionary<string, int> Points => _points.AsReadOnly();

    public GameStats(Dictionary<string, Option> actionOptions, Deck<ActionData> actions, Players.Repository players)
    {
        _actionOptions = actionOptions;
        _actions = actions;
        _players = players;
    }

    public void OnArrangementPurposed(string player, Arrangement arrangement)
    {
        foreach (string p in arrangement.Partners)
        {
            RegisterProposition(player, p);
        }

        if (!arrangement.CompatablePartners)
        {
            return;
        }
        foreach ((string, string) pair in ListHelper.EnumeratePairs(arrangement.Partners))
        {
            RegisterProposition(pair.Item1, pair.Item2);
        }
    }

    public void OnActionCompleted(string player, ushort id, IEnumerable<string> partners, bool fully)
    {
        ActionData data = _actions.GetCard(id);
        int? points = GetPoints(data.Tag, fully);
        if (points is null)
        {
            throw new NullReferenceException($"No points in config for ({data.Tag}, {fully})");
        }

        _points.CreateOrAdd(player, points.Value);
        foreach (string partner in partners)
        {
            _points.CreateOrAdd(partner, points.Value);
        }
    }

    public bool UpdateList(List<PlayerListUpdateData> updateDatas)
    {
        int minPoints = _points.Any() ? _points.Values.Min() : 0;
        bool changed = false;

        foreach (PlayerListUpdateData data in updateDatas)
        {
            switch (data)
            {
                case AddOrUpdatePlayerData a:
                    if (!_players.GetActiveNames().Contains(a.Name))
                    {
                        EnsureMinPoints(a.Name, minPoints);
                    }

                    changed |= _players.AddOrUpdatePlayerData(a);

                    break;
                case TogglePlayerData t:
                    if (_players.TogglePlayerData(t))
                    {
                        if (_players.GetActiveNames().Contains(t.Name))
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

    public int GetPropositions(string player, IReadOnlyList<string> players)
    {
        return players.Sum(p => GetPropositions(player, p))
               + ListHelper.EnumeratePairs(players)
                           .Sum(pair => GetPropositions(pair.Item1, pair.Item2));
    }

    public int GetPropositions(string p1, string p2)
    {
        string key = GetKey(p1, p2);
        return _propositions.GetValueOrDefault(key);
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

    private void RegisterProposition(string p1, string p2)
    {
        string key = GetKey(p1, p2);
        _propositions.CreateOrAdd(key, 1);
    }

    private static string GetKey(string p1, string p2)
    {
        return string.Compare(p1, p2, StringComparison.Ordinal) < 0 ? p1 + p2 : p2 + p1;
    }

    private readonly Dictionary<string, Option> _actionOptions;
    private readonly Deck<ActionData> _actions;
    private readonly Players.Repository _players;
    private readonly Dictionary<string, int> _points = new();
    private readonly Dictionary<string, int> _propositions = new();
}