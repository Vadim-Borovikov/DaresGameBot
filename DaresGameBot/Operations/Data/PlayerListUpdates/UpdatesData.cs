using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Configs;
using DaresGameBot.Game.States;
using GoogleSheetsManager.Extensions;

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
            string player;
            byte? index;
            string group;
            string[] groups;
            HashSet<string> compatableGroups;
            GroupsInfo info;
            switch (parts.Length)
            {
                case 1:
                    player = parts[0];
                    data = new TogglePlayerData(player);
                    break;
                case 2:
                    player = parts[0];
                    index = parts[1].ToByte();
                    if (index is null)
                    {
                        return null;
                    }
                    data = new TogglePlayerData(player, index);
                    break;
                case 3:
                    player = parts[0];
                    group = parts[1];
                    groups = parts[2].Split(texts.UpdateGroupsSeparator);
                    compatableGroups = new HashSet<string>(groups);
                    info = new GroupsInfo(group, compatableGroups);
                    data = new AddOrUpdatePlayerData(player, info);
                    break;
                case 4:
                    player = parts[0];
                    index = parts[1].ToByte();
                    if (index is null)
                    {
                        return null;
                    }
                    group = parts[2];
                    groups = parts[3].Split(texts.UpdateGroupsSeparator);
                    compatableGroups = new HashSet<string>(groups);
                    info = new GroupsInfo(group, compatableGroups);
                    data = new AddOrUpdatePlayerData(player, info, index);
                    break;
                default: return null;
            }

            datas.Add(data);
        }

        return new UpdatesData(datas);
    }
}