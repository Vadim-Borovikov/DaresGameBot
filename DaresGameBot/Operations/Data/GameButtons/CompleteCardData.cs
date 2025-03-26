using GoogleSheetsManager.Extensions;

namespace DaresGameBot.Operations.Data.GameButtons;

internal sealed class CompleteCardData
{
    public readonly bool? Fully;

    public CompleteCardData(string callbackQueryDataCore) => Fully = callbackQueryDataCore.ToBool();
}