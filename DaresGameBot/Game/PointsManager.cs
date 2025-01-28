using System.Collections.Generic;
using DaresGameBot.Game.Matchmaking.Interactions;
using DaresGameBot.Game.Players;

namespace DaresGameBot.Game;

internal sealed class PointsManager : IInteractionSubscriber
{
    public PointsManager(IReadOnlyDictionary<string, ushort> points, Repository players)
    {
        _points = points;
        _players = players;
    }

    public void OnInteractionCompleted(string player, Arrangement arrangement, string tag)
    {
        _players.AddPoints(player, _points[tag]);
        foreach (string partner in arrangement.Partners)
        {
            _players.AddPoints(partner, _points[tag]);
        }
    }

    private readonly IReadOnlyDictionary<string, ushort> _points;
    private readonly Repository _players;
}