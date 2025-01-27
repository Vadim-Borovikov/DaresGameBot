using System;
using DaresGameBot.Game.Data;

namespace DaresGameBot.Operations.Info;

internal abstract class GameButtonInfo
{
    public static GameButtonInfo? From(string callbackQueryDataCore)
    {
        if (callbackQueryDataCore == "")
        {
            return new GameButtonInfoQuestion();
        }

        string[] parts = callbackQueryDataCore.Split(FieldSeparator);
        if (parts.Length < 3)
        {
            return null;
        }
        string tag = parts[0];
        int hash = int.Parse(parts[1]);
        string[] partners = SplitList(parts[2]);
        ArrangementInfo info = new(hash, partners);
        switch (parts.Length)
        {
            case 3:
                return new GameButtonInfoArrangement(info, tag);
            case 5:
                ushort actionId = ushort.Parse(parts[3]);
                string[] helpers = SplitList(parts[4]);
                ActionInfo actionInfo = new(info, actionId, helpers);
                return new GameButtonInfoAction(actionInfo, tag);
            default: return null;
        }
    }

    private static string[] SplitList(string s) => s == "" ? Array.Empty<string>() : s.Split(ListSeparator);

    internal const string FieldSeparator = "|";
    internal const string ListSeparator = ";";
}