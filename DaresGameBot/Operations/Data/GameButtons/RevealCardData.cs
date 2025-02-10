using DaresGameBot.Game;

namespace DaresGameBot.Operations.Data.GameButtons;

internal abstract class RevealCardData : GameButtonData
{
    public static RevealCardData? From(string callbackQueryDataCore)
    {
        string[] parts = callbackQueryDataCore.Split(FieldSeparator);
        switch (parts.Length)
        {
            case 1: return new RevealQuestionData();
            case 3:
                Arrangement arrangement = GetArrangement(parts[0], parts[1]);
                string tag = parts[2];
                return new RevealActionData(arrangement, tag);
            default: return null;
        }
    }
}