using System;
using DaresGameBot.Game;

namespace DaresGameBot.Operations.Data.GameButtons;

internal abstract class GameButtonData
{
    protected static Arrangement GetArrangement(string left, string right)
    {
        string[] partners = SplitList(left);
        bool compatablePartners = bool.Parse(right);
        return new Arrangement(partners, compatablePartners);
    }

    private static string[] SplitList(string s) => s == "" ? Array.Empty<string>() : s.Split(ListSeparator);

    internal const string FieldSeparator = "|";
    internal const string ListSeparator = ";";
}