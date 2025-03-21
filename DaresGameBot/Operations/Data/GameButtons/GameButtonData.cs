using System.Linq;
using DaresGameBot.Context;
using DaresGameBot.Game;
using GoogleSheetsManager.Extensions;
using GryphonUtilities.Extensions;

namespace DaresGameBot.Operations.Data.GameButtons;

internal abstract class GameButtonData
{
    protected static Arrangement? TryGetArrangement(PlayersRepository players, string left, string right)
    {
        string[]? partners = SplitList(players, left);
        if (partners is null)
        {
            return null;
        }

        bool? compatablePartners = right.ToBool();
        return compatablePartners is null ? null : new Arrangement(partners, compatablePartners.Value);
    }

    private static string[]? SplitList(PlayersRepository players, string s)
    {
        return string.IsNullOrWhiteSpace(s)
            ? null
            : s.Split(PartnersSeparator).Select(p => p.ToInt()).SkipNulls().Select(players.NameAt).ToArray();
    }

    internal const string PartnersSeparator = ";";
    internal const string FieldSeparator = "|";
}