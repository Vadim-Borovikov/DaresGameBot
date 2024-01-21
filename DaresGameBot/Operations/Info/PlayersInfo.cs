using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Game.Data;
using GryphonUtilities.Extensions;

namespace DaresGameBot.Operations.Info;

internal sealed class PlayersInfo
{
    public readonly List<Player> Players;

    private PlayersInfo(List<Player> players) => Players = players;

    public static PlayersInfo? From(IEnumerable<string> parts)
    {
        List<Player>? players = parts.Select(Player.From).TryDenullAll();
        return players is null ? null : new PlayersInfo(players);
    }
}