using System;
using System.Collections.Generic;
using DaresGameBot.Configs;
using DaresGameBot.Game.Matchmaking.Interactions;
using DaresGameBot.Game.Players;

namespace DaresGameBot.Game;

internal sealed class PointsManager : IInteractionSubscriber
{
    public PointsManager(Dictionary<string, Option> actionOptions, Repository players)
    {
        _actionOptions = actionOptions;
        _players = players;
    }

    public ushort? GetPoints(string tag, bool partial)
    {
        if (!_actionOptions.ContainsKey(tag))
        {
            return null;
        }
        return partial ? _actionOptions[tag].PartialPoints : _actionOptions[tag].Points;
    }

    public void OnInteractionCompleted(string player, Arrangement arrangement, string tag, bool completedFully)
    {
        ushort? points = GetPoints(tag, !completedFully);
        if (points is null)
        {
            throw new NullReferenceException($"No points in config for ({tag}, {completedFully})");
        }

        _players.AddPoints(player, points.Value);
        foreach (string partner in arrangement.Partners)
        {
            _players.AddPoints(partner, points.Value);
        }
    }

    private readonly Dictionary<string, Option> _actionOptions;
    private readonly Repository _players;
}