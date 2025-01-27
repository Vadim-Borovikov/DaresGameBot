using System;
using DaresGameBot.Game.Data;

namespace DaresGameBot.Operations.Info;

internal abstract class GameButtonInfo
{
    public static GameButtonInfo? From(string callbackQueryDataCore)
    {
        string[] parts = callbackQueryDataCore.Split(FieldSeparator);
        if ((parts.Length == 0) || (parts[0] == string.Empty))
        {
            return null;
        }

        string tag;
        int hash;
        string[] partners;
        string[] helpers;
        ArrangementInfo info;
        switch (parts.Length)
        {
            case 1: return new GameButtonInfoQuestion();
            case 4:
                tag = parts[0];
                hash = int.Parse(parts[1]);
                partners = SplitList(parts[2]);
                helpers = SplitList(parts[3]);
                info = new ArrangementInfo(hash, partners, helpers);
                return new GameButtonInfoArrangement(info, tag);
            case 5:
                tag = parts[0];
                ushort actionId = ushort.Parse(parts[1]);
                hash = int.Parse(parts[2]);
                partners = SplitList(parts[3]);
                helpers = SplitList(parts[4]);
                info = new ArrangementInfo(hash, partners, helpers);
                ActionInfo actionInfo = new(info, actionId);
                return new GameButtonInfoAction(actionInfo, tag);
            default: return null;
        }
    }

    private static string[] SplitList(string s) => s == "" ? Array.Empty<string>() : s.Split(ListSeparator);

    internal const string FieldSeparator = "|";
    internal const string ListSeparator = ";";
}