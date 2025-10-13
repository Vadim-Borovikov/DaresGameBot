using DaresGameBot.Game.Matchmaking.Interactions;
using DaresGameBot.Game.States.Cores;
using DaresGameBot.Game.States.Data;
using DaresGameBot.Operations.Data.PlayerListUpdates;
using DaresGameBot.Utilities;
using DaresGameBot.Utilities.Extensions;
using GryphonUtilities.Save;
using System;
using System.Collections.Generic;

namespace DaresGameBot.Game.States;

internal sealed class GameStats : IInteractionSubscriber, IStateful<GameStatsData>
{
    public GameStats(GameStatsStateCore core) => _core = core;

    public void OnQuestionCompleted(string player, Arrangement? arrangement)
    {
        if (arrangement is null)
        {
            RegisterProposition(player);
        }
        else
        {
            RegisterPropositions(player, arrangement);
        }
        RegisterTurn();

        if (_core.QuestionPoints.HasValue)
        {
            RegisterPoints(player, arrangement, _core.QuestionPoints.Value);
        }
    }

    public void OnActionCompleted(string player, Arrangement arrangement, string tag, bool fully)
    {
        RegisterPropositions(player, arrangement);
        RegisterTurn();

        uint? points = GetPoints(tag, fully);
        if (points is null)
        {
            throw new NullReferenceException($"No points in config for ({tag}, {fully})");
        }
        RegisterPoints(player, arrangement, points.Value);
    }

    public bool UpdateList(List<PlayerListUpdateData> updateDatas)
    {
        bool changed = false;

        foreach (PlayerListUpdateData data in updateDatas)
        {
            switch (data)
            {
                case AddOrUpdatePlayerData a:
                    changed |= _core.Players.AddOrUpdatePlayerData(a);
                    break;
                case TogglePlayerData t:
                    changed |= _core.Players.TogglePlayerData(t);
                    break;
            }
        }

        return changed;
    }

    public uint GetPropositions(string player) => _propositions.GetValueOrDefault(player);

    public uint GetPropositions(string p1, string p2)
    {
        string key = GetKey(p1, p2);
        return _propositions.GetValueOrDefault(key);
    }

    public float? GetRatio(string player)
    {
        uint propositions = GetPropositions(player);
        return propositions == 0 ? null : 1.0f * GetPoints(player) / propositions;
    }

    public uint GetPoints(string player) => _points.GetValueOrDefault(player);
    public uint GetTurns(string player) => _turns.GetValueOrDefault(player);

    public GameStatsData Save()
    {
        return new GameStatsData
        {
            Points = _points,
            Propositions = _propositions,
            Turns = _turns
        };
    }

    public void LoadFrom(GameStatsData? data)
    {
        if (data is null)
        {
            return;
        }

        _points.Clear();
        _points.AddAll(data.Points);

        _propositions.Clear();
        _propositions.AddAll(data.Propositions);

        _turns.Clear();
        _turns.AddAll(data.Turns);
    }

    private ushort? GetPoints(string? tag, bool completedFully)
    {
        if (string.IsNullOrWhiteSpace(tag) || !_core.ActionOptions.ContainsKey(tag))
        {
            return null;
        }
        return completedFully ? _core.ActionOptions[tag].Points : _core.ActionOptions[tag].PartialPoints;
    }

    private void RegisterPoints(string player, Arrangement? arrangement, uint points)
    {
        _points.CreateOrAdd(player, points);

        if (arrangement is null)
        {
            return;
        }
        foreach (string partner in arrangement.Partners)
        {
            _points.CreateOrAdd(partner, points);
        }
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
        foreach (string player in _core.Players.GetActiveNames())
        {
            _turns.CreateOrAdd(player, 1);
        }
    }

    private readonly Dictionary<string, uint> _points = new();
    private readonly Dictionary<string, uint> _propositions = new();
    private readonly Dictionary<string, uint> _turns = new();

    private readonly GameStatsStateCore _core;
}