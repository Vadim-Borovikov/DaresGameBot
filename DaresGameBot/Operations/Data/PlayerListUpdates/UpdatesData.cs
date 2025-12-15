using System;
using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Configs;
using DaresGameBot.Game.States;

namespace DaresGameBot.Operations.Data.PlayerListUpdates;

internal sealed class UpdatesData
{
    public readonly List<AddOrUpdatePlayerData> Datas;

    private UpdatesData(List<AddOrUpdatePlayerData> datas) => Datas = datas;

    public static UpdatesData? From(IEnumerable<string> lines, Texts texts)
    {
        List<AddOrUpdatePlayerData> datas = new();
        foreach (string[] parts in
                 lines.Select(l => l.Split(texts.UpdatePartsSeparator, StringSplitOptions.TrimEntries)))
        {
            if (parts.Length != 3)
            {
                return null;
            }

            string player = parts[0];
            string group = parts[1];
            string[] groups = parts[2].Split(texts.UpdateGroupsSeparator);
            HashSet<string> compatableGroups = new(groups);
            GroupsInfo info = new(group, compatableGroups);
            AddOrUpdatePlayerData data = new(player, info);

            datas.Add(data);
        }

        return new UpdatesData(datas);
    }
}