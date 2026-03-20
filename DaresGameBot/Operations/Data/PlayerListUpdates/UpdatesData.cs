using System;
using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Configs;
using DaresGameBot.Game.States;
using GoogleSheetsManager.Extensions;
using GryphonUtilities.Extensions;

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
            if (parts.Length is not 4 and not 5)
            {
                return null;
            }

            string name = parts[0];
            string? username = null;
            if ((parts.Length == 5) && !string.IsNullOrWhiteSpace(parts[1]))
            {
                username = parts[1];
            }
            HashSet<byte> rounds = new(parts[parts.Length - 3].Split(texts.UpdatePartSeparator)
                                                              .Select(s => s.ToByte())
                                                              .SkipNulls());
            string group = parts[parts.Length - 2];
            string[] groups = parts[parts.Length - 1].Split(texts.UpdatePartSeparator);
            HashSet<string> compatableGroups = new(groups);
            GroupsInfo info = new(group, compatableGroups);

            AddOrUpdatePlayerData data = new(name, username, rounds, info);

            datas.Add(data);
        }

        return new UpdatesData(datas);
    }
}