using System;
using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Configs;
using DaresGameBot.Game.States;
using GoogleSheetsManager.Extensions;

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
            if (parts.Length != 4)
            {
                return null;
            }

            long? id = parts[0].ToLong();
            if (id is null)
            {
                return null;
            }

            string player = parts[1];
            string group = parts[2];
            string[] groups = parts[3].Split(texts.UpdateGroupsSeparator);
            HashSet<string> compatableGroups = new(groups);
            GroupsInfo info = new(group, compatableGroups);

            AddOrUpdatePlayerData data = new(id.Value, player, info);

            datas.Add(data);
        }

        return new UpdatesData(datas);
    }
}