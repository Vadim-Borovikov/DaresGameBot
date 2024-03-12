using DaresGameBot.Game.Matchmaking.PlayerCheck;
using System.Collections.Generic;
using System.Linq;

namespace DaresGameBot.Operations.Info;

internal sealed class PlayersInfo
{
    public readonly List<string> Players;
    public readonly Dictionary<string, IPartnerChecker> InteractabilityInfos;

    private PlayersInfo(List<string> players, Dictionary<string, IPartnerChecker> interactabilityInfos)
    {
        Players = players;
        InteractabilityInfos = interactabilityInfos;
    }

    public static PlayersInfo? From(IEnumerable<string> lines)
    {
        List<string> players = new();
        Dictionary<string, IPartnerChecker> compatibilityInfos = new();

        foreach (string[] parts in lines.Select(l => l.Split(PartsSeparator)))
        {
            if (parts.Length != 3)
            {
                return null;
            }

            string player = parts[0];
            string group = parts[1];
            string[] groups = parts[2].Split(GroupsSeparator);
            HashSet<string> compatableGroups = new(groups);

            players.Add(player);
            compatibilityInfos[player] = new GroupChecker(group, new HashSet<string>(compatableGroups));
        }

        return new PlayersInfo(players, compatibilityInfos);
    }

    private const string PartsSeparator = ",";
    private const string GroupsSeparator = "+";
}