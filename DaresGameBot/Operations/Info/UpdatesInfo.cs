using DaresGameBot.Game.Data.PlayerListUpdates;
using DaresGameBot.Game.Matchmaking.PlayerCheck;
using System.Collections.Generic;
using System.Linq;

namespace DaresGameBot.Operations.Info;

internal sealed class UpdatesInfo
{
    public readonly List<PlayerListUpdate> Updates;

    private UpdatesInfo(List<PlayerListUpdate> updates) => Updates = updates;

    public static UpdatesInfo? From(IEnumerable<string> lines)
    {
        List<PlayerListUpdate> updates = new();
        foreach (string[] parts in lines.Select(l => l.Split(PartsSeparator)))
        {
            PlayerListUpdate update;
            switch (parts.Length)
            {
                case 1:
                    update = new RemovePlayer(parts[0]);
                    break;
                case 3:
                    string player = parts[0];
                    string group = parts[1];
                    string[] groups = parts[2].Split(GroupsSeparator);
                    HashSet<string> compatableGroups = new(groups);
                    GroupChecker checker = new(group, new HashSet<string>(compatableGroups));
                    update = new AddOrUpdatePlayer(player, checker);
                    break;
                default: return null;
            }

            updates.Add(update);
        }

        return new UpdatesInfo(updates);
    }

    private const string PartsSeparator = ",";
    private const string GroupsSeparator = "+";
}