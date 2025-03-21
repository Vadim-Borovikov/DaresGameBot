using DaresGameBot.Context;
using DaresGameBot.Game;

namespace DaresGameBot.Operations.Data.GameButtons;

internal abstract class RevealCardData : GameButtonData
{
    public readonly Arrangement Arrangement;

    protected RevealCardData(Arrangement arrangement) => Arrangement = arrangement;

    public static RevealCardData? From(string callbackQueryDataCore, PlayersRepository players)
    {
        string[] parts = callbackQueryDataCore.Split(FieldSeparator);
        if (parts.Length < 2)
        {
            return null;
        }

        Arrangement? arrangement = TryGetArrangement(players, parts[0], parts[1]);
        if (arrangement is null)
        {
            return null;
        }

        switch (parts.Length)
        {
            case 2: return new RevealQuestionData(arrangement);
            case 3:
                string tag = parts[2];
                return new RevealActionData(arrangement, tag);
            default: return null;
        }
    }
}