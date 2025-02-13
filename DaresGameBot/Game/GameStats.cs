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
    public GameStats(Dictionary<string, Option> actionOptions, Deck<ActionData> actions, Players.Repository players)
    {
        _actionOptions = actionOptions;
        _actions = actions;
        _players = players;
    }

    public void OnQuestionCompleted(string player, Arrangement? declinedArrangement)
    {
        if (declinedArrangement is null)
        {
            RegisterProposition(player);
        }
        else
        {
            RegisterPropositions(player, declinedArrangement);
        }
        RegisterTurn();
    }

    public void OnActionCompleted(string player, ActionInfo info, bool fully)
    {
        RegisterPropositions(player, info.Arrangement);
        RegisterTurn();

        ActionData data = _actions.GetCard(info.Id);
        int? points = GetPoints(data.Tag, fully);
        if (points is null)
        {
            throw new NullReferenceException($"No points in config for ({data.Tag}, {fully})");
        }

        _points.CreateOrAdd(player, points.Value);
        foreach (string partner in info.Arrangement.Partners)
        {
            _points.CreateOrAdd(partner, points.Value);
        }
    }

    public bool UpdateList(List<PlayerListUpdateData> updateDatas)
    {
        bool changed = false;

        foreach (PlayerListUpdateData data in updateDatas)
        {
            switch (data)
            {
                case AddOrUpdatePlayerData a:
                    changed |= _players.AddOrUpdatePlayerData(a);
                    break;
                case TogglePlayerData t:
                    changed |= _players.TogglePlayerData(t);
                    break;
            }
        }

        return changed;
    }

    public int GetPropositions(string player) => _propositions.GetValueOrDefault(player);

    public int GetPropositions(string p1, string p2)
    {
        string key = GetKey(p1, p2);
        return _propositions.GetValueOrDefault(key);
    }

    public int GetPropositions(string player, IReadOnlyList<string> players)
    {
        return players.Sum(p => GetPropositions(player, p))
               + ListHelper.EnumeratePairs(players)
                           .Sum(pair => GetPropositions(pair.Item1, pair.Item2));
    }

    public float? GetRatio(string player)
    {
        int propositions = GetPropositions(player);
        return propositions == 0 ? null : 1.0f * _points.GetValueOrDefault(player) / propositions;
    }

    public int GetTurns(string player) => _turns.GetValueOrDefault(player);

    private int? GetPoints(string tag, bool completedFully)
    {
        if (!_actionOptions.ContainsKey(tag))
        {
            return null;
        }
        return completedFully ? _actionOptions[tag].Points : _actionOptions[tag].PartialPoints;
    }

    private void RegisterPropositions(string player, Arrangement arrangement)
    {
        RegisterProposition(player);

        foreach (string p in arrangement.Partners)
        {
            RegisterProposition(p);
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

    private void RegisterProposition(string key) => _propositions.CreateOrAdd(key, 1);

    private void RegisterProposition(string p1, string p2)
    {
        string key = GetKey(p1, p2);
        RegisterProposition(key);
    }

    private static string GetKey(string p1, string p2)
    {
        return string.Compare(p1, p2, StringComparison.Ordinal) < 0 ? p1 + p2 : p2 + p1;
    }

    private void RegisterTurn()
    {
        foreach (string player in _players.GetActiveNames())
        {
            _turns.CreateOrAdd(player, 1);
        }
    }

    private readonly Dictionary<string, Option> _actionOptions;
    private readonly Deck<ActionData> _actions;
    private readonly Players.Repository _players;
    private readonly Dictionary<string, int> _points = new();
    private readonly Dictionary<string, int> _propositions = new();
    private readonly Dictionary<string, int> _turns = new();
}