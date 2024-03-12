using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Game.Data.Players;
using DaresGameBot.Game.Matchmaking.PlayerCheck;

namespace DaresGameBot.Operations.Info;

internal sealed class PlayersInfo
{
    public readonly List<Player> Players;
    public readonly Dictionary<string, IPartnerChecker> InteractabilityInfos;

    private PlayersInfo(List<Player> players, Dictionary<string, IPartnerChecker> interactabilityInfos)
    {
        Players = players;
        InteractabilityInfos = interactabilityInfos;
    }

    public static PlayersInfo? From(IEnumerable<string> lines)
    {
        List<Player> players = new();
        Dictionary<string, IPartnerChecker> compatibilityInfos = new();

        foreach (string[] parts in lines.Select(l => l.Split(PartsSeparator)))
        {
            if (parts.Length != 3)
            {
                return null;
            }

            string name = parts[0];
            string group = parts[1];
            string[] compatableGroups = parts[2].Split(GroupsSeparator);

            Player player = new(name);
            GroupChecker info = new(group, new HashSet<string>(compatableGroups));

            players.Add(player);
            compatibilityInfos[name] = info;
        }

        return new PlayersInfo(players, compatibilityInfos);
    }

    private const string PartsSeparator = ",";
    private const string GroupsSeparator = "+";
}