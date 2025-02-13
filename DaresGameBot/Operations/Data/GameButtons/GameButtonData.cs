using DaresGameBot.Game;
using GoogleSheetsManager.Extensions;

namespace DaresGameBot.Operations.Data.GameButtons;

internal abstract class GameButtonData
{
    protected static Arrangement? TryGetArrangement(string left, string right)
    {
        string[]? partners = SplitList(left);
        if (partners is null)
        {
            return null;
        }

        bool? compatablePartners = right.ToBool();
        return compatablePartners is null ? null : new Arrangement(partners, compatablePartners.Value);
    }

    private static string[]? SplitList(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Split(ListSeparator);

    internal const string FieldSeparator = "|";
    internal const string ListSeparator = ";";
}