using DaresGameBot.Game;

namespace DaresGameBot.Operations.Data.GameButtons;

internal abstract class CompleteCardData : GameButtonData
{
    public static CompleteCardData? From(string callbackQueryDataCore)
    {
        string[] parts = callbackQueryDataCore.Split(FieldSeparator);
        switch (parts.Length)
        {
            case 1:
                return ushort.TryParse(parts[0], out ushort questionId)
                    ? new CompleteQuestionData(questionId)
                    : null;
            case 4:
                Arrangement arrangement = GetArrangement(parts[0], parts[1]);
                ushort actionId = ushort.Parse(parts[2]);
                ActionInfo actionInfo = new(actionId, arrangement);
                bool completedFully = bool.Parse(parts[3]);
                return new CompleteActionData(actionInfo, completedFully);
            default: return null;
        }
    }
}