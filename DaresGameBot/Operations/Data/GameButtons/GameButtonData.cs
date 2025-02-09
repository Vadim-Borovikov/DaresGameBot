using System;
using DaresGameBot.Game;

namespace DaresGameBot.Operations.Data.GameButtons;

internal abstract class GameButtonData
{
    public static GameButtonData? From(string callbackQueryDataCore)
    {
        string[] parts = callbackQueryDataCore.Split(FieldSeparator);
        switch (parts.Length)
        {
            case 1:
                ushort questionId = ushort.Parse(parts[0]);
                return new GameButtonQuestionData(questionId);
            case < 3: return null;
        }

        string[] partners = SplitList(parts[0]);
        bool compatablePartners = bool.Parse(parts[1]);
        Arrangement arrangement = new(partners, compatablePartners);
        switch (parts.Length)
        {
            case 3:
                string tag = parts[2];
                return new GameButtonArrangementData(arrangement, tag);
            case 4:
                ushort actionId = ushort.Parse(parts[2]);
                ActionInfo actionInfo = new(actionId, arrangement);
                bool completedFully = bool.Parse(parts[3]);
                return new GameButtonActionData(actionInfo, completedFully);
            default: return null;
        }
    }

    private static string[] SplitList(string s) => s == "" ? Array.Empty<string>() : s.Split(ListSeparator);

    internal const string FieldSeparator = "|";
    internal const string ListSeparator = ";";
}