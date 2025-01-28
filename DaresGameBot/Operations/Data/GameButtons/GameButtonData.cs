using System;
using DaresGameBot.Game;

namespace DaresGameBot.Operations.Data.GameButtons;

internal abstract class GameButtonData
{
    public static GameButtonData? From(string callbackQueryDataCore)
    {
        if (callbackQueryDataCore == "")
        {
            return new GameButtonQuestionData();
        }

        string[] parts = callbackQueryDataCore.Split(FieldSeparator);
        if (parts.Length != 3)
        {
            return null;
        }
        string[] partners = SplitList(parts[0]);
        bool compatablePartners = bool.Parse(parts[1]);
        Arrangement arrangement = new(partners, compatablePartners);
        if (ushort.TryParse(parts[2], out ushort actionId))
        {
            ActionInfo actionInfo = new(actionId, arrangement);
            return new GameButtonActionData(actionInfo);
        }
        string tag = parts[2];
        return new GameButtonArrangementData(arrangement, tag);
    }

    private static string[] SplitList(string s) => s == "" ? Array.Empty<string>() : s.Split(ListSeparator);

    internal const string FieldSeparator = "|";
    internal const string ListSeparator = ";";
}