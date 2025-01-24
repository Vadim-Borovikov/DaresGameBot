using DaresGameBot.Game.Matchmaking.PlayerCheck;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaresGameBot.Operations.Info;

internal sealed class PlayersInfo
{
    public readonly List<string> Lines;
    public readonly List<string> Players;
    public readonly Dictionary<string, IPartnerChecker> InteractabilityInfos;

    private PlayersInfo(List<string> players, Dictionary<string, GroupChecker> interactabilityInfos)
    {
        Players = players;

        Lines = new List<string>();
        InteractabilityInfos = new Dictionary<string, IPartnerChecker>();
        foreach (string player in Players)
        {
            GroupChecker checker = interactabilityInfos[player];

            StringBuilder lineBuilder = new();
            lineBuilder.Append(player);
            lineBuilder.Append(PartsSeparator);
            lineBuilder.Append(checker.Group);
            lineBuilder.Append(PartsSeparator);
            lineBuilder.Append(string.Join(GroupsSeparator, checker.CompatableGroups));

            Lines.Add(lineBuilder.ToString());
            InteractabilityInfos[player] = checker;
        }
    }

    public static PlayersInfo? From(IEnumerable<string> lines)
    {
        List<string> players = new();
        Dictionary<string, GroupChecker> compatibilityInfos = new();

        foreach (string[] parts in lines.Select(l => l.Split(PartsSeparator)))
        {
            string player;
            switch (parts.Length)
            {
                case 1:
                    player = parts[0];
                    players.Remove(player);
                    compatibilityInfos.Remove(player);
                    break;
                case 3:
                    player = parts[0];
                    string group = parts[1];
                    string[] groups = parts[2].Split(GroupsSeparator);
                    HashSet<string> compatableGroups = new(groups);

                    if (!players.Contains(player))
                    {
                        players.Add(player);
                    }
                    compatibilityInfos[player] = new GroupChecker(group, new HashSet<string>(compatableGroups));
                    break;
                default: return null;
            }
        }

        return new PlayersInfo(players, compatibilityInfos);
    }

    private const string PartsSeparator = ",";
    private const string GroupsSeparator = "+";
}