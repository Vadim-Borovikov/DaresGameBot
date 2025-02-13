using DaresGameBot.Game;
using DaresGameBot.Helpers;
using GoogleSheetsManager.Extensions;

namespace DaresGameBot.Operations.Data.GameButtons;

internal abstract class CompleteCardData : GameButtonData
{
    public static CompleteCardData? From(string callbackQueryDataCore)
    {
        string[] parts = callbackQueryDataCore.Split(FieldSeparator);

        switch (parts.Length)
        {
            case 1:
                ushort? questionId = parts[0].ToUshort();
                return questionId is null ? null : new CompleteQuestionData(questionId.Value);
            case 4:
                Arrangement? arrangement = TryGetArrangement(parts[0], parts[1]);
                if (arrangement is null)
                {
                    return null;
                }

                ushort? actionId = parts[2].ToUshort();
                if (actionId is null)
                {
                    return null;
                }

                ActionInfo actionInfo = new(actionId.Value, arrangement);

                bool? completedFully = parts[3].ToBool();
                return completedFully is null ? null : new CompleteActionData(actionInfo, completedFully.Value);
            default: return null;
        }
    }
}