using DaresGameBot.Game.Matchmaking.Interactions;
using DaresGameBot.Game.States.Cores;
using DaresGameBot.Game.States.Data;
using DaresGameBot.Operations.Data.PlayerListUpdates;
using DaresGameBot.Utilities;
using DaresGameBot.Utilities.Extensions;
using GryphonUtilities.Save;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DaresGameBot.Game.States;

internal sealed class GameStats : IInteractionSubscriber, IStateful<GameStatsData>
{
    public uint? MinRound { get; private set; }

    public GameStats(GameStatsStateCore core) => _core = core;

    public void OnQuestionCompleted(long player, Arrangement? arrangement, List<long> activePlayers)
    {
        if (arrangement is null)
        {
            RegisterProposition(player);
        }
        else
        {
            RegisterPropositions(player, arrangement);
        }
        RegisterTurn(player, activePlayers);

        if (_core.QuestionPoints.HasValue)
        {
            RegisterPoints(player, arrangement, _core.QuestionPoints.Value);
        }
    }

    public void OnActionCompleted(long player, Arrangement arrangement, List<long> activePlayers, string tag,
        bool fully)
    {
        RegisterPropositions(player, arrangement);
        RegisterTurn(player, activePlayers);

        uint? points = GetPoints(tag, fully)
                       ?? throw new NullReferenceException($"No points in config for ({tag}, {fully})");
        RegisterPoints(player, arrangement, points.Value);
    }

    public bool UpdateList(List<AddOrUpdatePlayerData> updateDatas)
    {
        bool changed = false;

        foreach (AddOrUpdatePlayerData data in updateDatas)
        {
            changed |= _core.Players.AddOrUpdatePlayerData(data);
        }

        return changed;
    }

    public float GetPartnerPropositionsRate(long player)
    {
        uint propositions =  _partnerPropositions.GetValueOrDefault(player);
        uint turns = GetTurns(player);

        if (propositions == 0)
        {
            return -turns;
        }

        if (turns == 0)
        {
            throw new InvalidOperationException($"Player {player} has propositions but no turns");
        }

        return 1.0f * propositions / turns;
    }

    public uint GetPropositions(long player) => _propositions.GetValueOrDefault(player.ToString());

    public uint GetPropositions(long p1, long p2)
    {
        string key = GetKey(p1, p2);
        return _propositions.GetValueOrDefault(key);
    }

    public uint? GetRatio(long player)
    {
        uint propositions = GetPropositions(player);
        return propositions == 0 ? null : GetPoints(player) / propositions;
    }

    public uint GetPoints(long player) => _points.GetValueOrDefault(player);
    public uint GetTurns(long player) => _turns.GetValueOrDefault(player);

    public GameStatsData Save()
    {
        return new GameStatsData
        {
            Points = _points,
            Propositions = _propositions,
            PartnerPropositions = _partnerPropositions,
            Turns = _turns,
            CurrentRound = _currentRound,
            MinRound = MinRound,
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

        _partnerPropositions.Clear();
        _partnerPropositions.AddAll(data.PartnerPropositions);

        _turns.Clear();
        _turns.AddAll(data.Turns);

        _currentRound = data.CurrentRound;
        MinRound = data.MinRound;
    }

    private ushort? GetPoints(string? tag, bool completedFully)
    {
        if (string.IsNullOrWhiteSpace(tag) || !_core.ActionOptions.ContainsKey(tag))
        {
            return null;
        }
        return completedFully ? _core.ActionOptions[tag].Points : _core.ActionOptions[tag].PartialPoints;
    }

    private void RegisterPoints(long player, Arrangement? arrangement, uint points)
    {
        _points.CreateOrAdd(player, points);

        if (arrangement is null)
        {
            return;
        }
        foreach (long partner in arrangement.Partners)
        {
            _points.CreateOrAdd(partner, points);
        }
    }

    private void RegisterPropositions(long player, Arrangement arrangement)
    {
        RegisterProposition(player);

        foreach (long p in arrangement.Partners)
        {
            RegisterProposition(p);
            RegisterProposition(player, p);
            RegisterPartnerProposition(p);
        }

        if (!arrangement.CompatablePartners)
        {
            return;
        }
        foreach ((long, long) pair in ListHelper.EnumeratePairs(arrangement.Partners))
        {
            RegisterProposition(pair.Item1, pair.Item2);
        }
    }

    private void RegisterPartnerProposition(long key) => _partnerPropositions.CreateOrAdd(key, 1);

    private void RegisterProposition(long id) => RegisterProposition(id.ToString());
    private void RegisterProposition(string key) => _propositions.CreateOrAdd(key, 1);

    private void RegisterProposition(long p1, long p2)
    {
        string key = GetKey(p1, p2);
        RegisterProposition(key);
    }

    private static string GetKey(long p1, long p2)
    {
        string s1 = p1.ToString();
        string s2 = p2.ToString();
        return p1 < p2 ? s1 + s2 : s2 + s1;
    }

    private void RegisterTurn(long player, List<long> activePlayers)
    {
        foreach (long activePlayer in activePlayers)
        {
            _turns.CreateOrAdd(activePlayer, 1);
        }

        RegisterRound(player == activePlayers.LastOrDefault());
    }

    private void RegisterRound(bool isLastPlayer)
    {
        ++_currentRound;

        if (!isLastPlayer)
        {
            return;
        }

        if (MinRound is null || (_currentRound < MinRound.Value))
        {
            MinRound = _currentRound;
        }

        _currentRound = 0;
    }

    private readonly Dictionary<long, uint> _points = new();
    private readonly Dictionary<string, uint> _propositions = new();
    private readonly Dictionary<long, uint> _partnerPropositions = new();
    private readonly Dictionary<long, uint> _turns = new();
    private readonly GameStatsStateCore _core;

    private uint _currentRound;
}