using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Configs;
using DaresGameBot.Game.States;

namespace DaresGameBot.Operations.Data.PlayerListUpdates;

internal sealed class UpdatesData
{
    public readonly List<PlayerListUpdateData> Datas;

    private UpdatesData(List<PlayerListUpdateData> datas) => Datas = datas;

    public static UpdatesData? From(IEnumerable<string> lines, Texts texts)
    {
        List<PlayerListUpdateData> datas = new();
        foreach (string[] parts in lines.Select(l => l.Split(texts.UpdatePartsSeparator)))
        {
            PlayerListUpdateData data;
            switch (parts.Length)
            {
                case 1:
                    data = new TogglePlayerData(parts[0]);
                    break;
                case 3:
                    string player = parts[0];
                    string group = parts[1];
                    string[] groups = parts[2].Split(texts.UpdateGroupsSeparator);
                    HashSet<string> compatableGroups = new(groups);
                    GroupsInfo info = new(group, compatableGroups);
                    data = new AddOrUpdatePlayerData(player, info);
                    break;
                default: return null;
            }

            datas.Add(data);
        }

        return new UpdatesData(datas);
    }
}