using AbstractBot.Models.MessageTemplates;
using GoogleSheetsManager.Extensions;

namespace DaresGameBot.Operations.Data.GameButtons;

internal sealed class CompleteCardData
{
    public readonly bool? Fully;
    public readonly MessageTemplateText Template;
    public readonly bool HasPhoto;

    private CompleteCardData(bool? fully, MessageTemplateText template, bool hasPhoto)
    {
        Fully = fully;
        Template = template;
        HasPhoto = hasPhoto;
    }

    public static CompleteCardData? From(string? markdown, bool hasPhoto, string callbackQueryDataCore)
    {
        if (string.IsNullOrEmpty(markdown))
        {
            return null;
        }

        bool? fully = callbackQueryDataCore.ToBool();
        MessageTemplateText template = new(markdown, true);
        return new CompleteCardData(fully, template, hasPhoto);
    }
}