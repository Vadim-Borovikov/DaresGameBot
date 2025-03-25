using DaresGameBot.Game;
using DaresGameBot.Game.States;

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

        Arrangement? arrangement = TryGetArrangement(parts[0], parts[1], players);
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