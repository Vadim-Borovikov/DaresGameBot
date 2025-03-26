using DaresGameBot.Utilities;
using DaresGameBot.Utilities.Extensions;
using GoogleSheetsManager.Extensions;

namespace DaresGameBot.Operations.Data.GameButtons;

internal abstract class CompleteCardData
{
    public readonly ushort Id;

    protected CompleteCardData(ushort id) => Id = id;

    public static CompleteCardData? From(string callbackQueryDataCore)
    {
        string[] parts = callbackQueryDataCore.Split(TextHelper.FieldSeparator);

        if (parts.Length == 0)
        {
            return null;
        }

        ushort? id = parts[0].ToUshort();
        if (id is null)
        {
            return null;
        }

        switch (parts.Length)
        {
            case 1: return new CompleteQuestionData(id.Value);
            case 2:
                bool? completedFully = parts[1].ToBool();
                return completedFully is null ? null : new CompleteActionData(id.Value, completedFully.Value);
            default: return null;
        }
    }
}