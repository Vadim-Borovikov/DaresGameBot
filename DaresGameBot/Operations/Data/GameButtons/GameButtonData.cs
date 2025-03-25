using System.Linq;
using DaresGameBot.Game;
using DaresGameBot.Game.States;
using GoogleSheetsManager.Extensions;
using GryphonUtilities.Extensions;

namespace DaresGameBot.Operations.Data.GameButtons;

internal abstract class GameButtonData
{
    protected static Arrangement? TryGetArrangement(string left, string right, PlayersRepository players)
    {
        string[]? partners = SplitList(left, players);
        if (partners is null)
        {
            return null;
        }

        bool? compatablePartners = right.ToBool();
        return compatablePartners is null ? null : new Arrangement(partners, compatablePartners.Value);
    }

    private static string[]? SplitList(string s, PlayersRepository players)
    {
        return string.IsNullOrWhiteSpace(s)
            ? null
            : s.Split(PartnersSeparator).Select(p => p.ToInt()).SkipNulls().Select(players.NameAt).ToArray();
    }

    internal const string PartnersSeparator = ";";
    internal const string FieldSeparator = "|";
}