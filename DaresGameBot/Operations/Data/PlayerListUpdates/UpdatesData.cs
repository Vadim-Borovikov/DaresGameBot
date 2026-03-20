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
            if (parts.Length is not 3 and not 4)
            {
                return null;
            }

            string name = parts[0];
            string? username = null;
            if ((parts.Length == 4) && !string.IsNullOrWhiteSpace(parts[1]))
            {
                username = parts[1];
            }
            string group = parts[parts.Length - 2];
            string[] groups = parts[parts.Length - 1].Split(texts.UpdateGroupsSeparator);
            HashSet<string> compatableGroups = new(groups);
            GroupsInfo info = new(group, compatableGroups);

            AddOrUpdatePlayerData data = new(name, username, info);

            datas.Add(data);
        }

        return new UpdatesData(datas);
    }
}