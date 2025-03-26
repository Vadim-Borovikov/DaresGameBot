using GoogleSheetsManager.Extensions;

namespace DaresGameBot.Operations.Data.GameButtons;

internal sealed class CompleteCardData
{
    public readonly bool? Fully;
    public readonly string MessageText;

    private CompleteCardData(bool? fully, string messageText)
    {
        Fully = fully;
        MessageText = messageText;
    }

    public static CompleteCardData? From(string? messageText, string callbackQueryDataCore)
    {
        if (string.IsNullOrEmpty(messageText))
        {
            return null;
        }

        bool? fully = callbackQueryDataCore.ToBool();
        return new CompleteCardData(fully, messageText);
    }
}