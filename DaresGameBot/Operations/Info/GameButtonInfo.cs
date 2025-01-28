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
        string[] partners = SplitList(parts[1]);
        bool compatablePartners = bool.Parse(parts[2]);
        Arrangement arrangement = new(partners, compatablePartners);
        switch (parts.Length)
        {
            case 3:
                return new GameButtonInfoArrangement(arrangement, tag);
            case 4:
                ushort actionId = ushort.Parse(parts[3]);
                ActionInfo actionInfo = new(arrangement, actionId);
                return new GameButtonInfoAction(actionInfo, tag);
            default: return null;
        }
    }

    private static string[] SplitList(string s) => s == "" ? Array.Empty<string>() : s.Split(ListSeparator);

    internal const string FieldSeparator = "|";
    internal const string ListSeparator = ";";
}