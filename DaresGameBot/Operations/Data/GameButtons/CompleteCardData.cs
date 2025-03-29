using AbstractBot.Models.MessageTemplates;
using GoogleSheetsManager.Extensions;

namespace DaresGameBot.Operations.Data.GameButtons;

internal sealed class CompleteCardData
{
    public readonly bool? Fully;
    public readonly MessageTemplateText Template;

    private CompleteCardData(bool? fully, MessageTemplateText template)
    {
        Fully = fully;
        Template = template;
    }

    public static CompleteCardData? From(string? markdown, string callbackQueryDataCore)
    {
        if (string.IsNullOrEmpty(markdown))
        {
            return null;
        }

        bool? fully = callbackQueryDataCore.ToBool();
        MessageTemplateText template = new(markdown, true);
        return new CompleteCardData(fully, template);
    }
}