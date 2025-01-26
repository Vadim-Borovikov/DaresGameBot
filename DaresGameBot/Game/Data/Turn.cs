using System.Collections.Generic;
using System.IO;
using AbstractBot.Configs.MessageTemplates;
using DaresGameBot.Configs;

namespace DaresGameBot.Game.Data;

internal sealed class Turn
{
    public Turn(Texts texts, string imagesfolder, string tag, string descriprionRu, string? descriptionEn,
        ActionInfo actionInfo, bool compatablePartners, string? imagePath = null)
        : this(texts, imagesfolder, tag, descriprionRu, descriptionEn, actionInfo.Player, actionInfo,
            compatablePartners, imagePath)
    {
    }

    public Turn(Texts texts, string imagesfolder, string tag, string descriprionRu, string? descriptionEn,
        string player, string? imagePath = null)
        : this(texts, imagesfolder, tag, descriprionRu, descriptionEn, player, null, false, imagePath)
    {
    }

    private Turn(Texts texts, string imagesfolder, string tag, string descriprionRu, string? descriptionEn,
        string player, ActionInfo? actionInfo = null, bool compatablePartners = false, string? imagePath = null)
    {
        _texts = texts;
        _imagesfolder = imagesfolder;
        _player = player;
        _actionInfo = actionInfo;
        _compatablePartners = compatablePartners;
        _tagPart = new MessageTemplateText(tag);
        _descriprionRuPart = new MessageTemplateText(descriprionRu);
        _descriprionEnPart = descriptionEn is null ? null : new MessageTemplateText(descriptionEn);
        _imagePath = imagePath;
    }

    public MessageTemplate GetMessage(bool includeEn = false)
    {
        MessageTemplateText descriprionPart = _descriprionRuPart;
        if (includeEn && _descriprionEnPart is not null)
        {
            descriprionPart = _texts.TurnDescriptionRuEnFormat.Format(_descriprionRuPart, _descriprionEnPart);
        }

        MessageTemplate message = _texts.TurnFormatFull;
        string? partnersPart = null;
        IReadOnlyList<string>? partners = _actionInfo?.Partners;
        if (partners is not null && (partners.Count != 0))
        {
            partnersPart = GetPartnersPart(_texts, partners, _compatablePartners);
        }

        if (!string.IsNullOrWhiteSpace(_imagePath))
        {
            string path = Path.Combine(_imagesfolder, _imagePath);
            message = new MessageTemplateImage(message, path);
        }

        return message.Format(_tagPart, _player, descriprionPart, partnersPart);
    }

    public static string GetPartnersPart(Texts texts, IReadOnlyList<string> partners, bool compatable)
    {
        string partnersPrefix = partners.Count > 1 ? texts.Partners : texts.Partner;
        string separator = compatable ? texts.CompatablePartnersSeparator : texts.PartnersSeparator;
        string partnersText = string.Join(separator, partners);
        MessageTemplateText template = texts.TurnPartnersFormat.Format(partnersPrefix, partnersText);
        return template.EscapeIfNeeded();
    }

    private readonly MessageTemplateText _tagPart;
    private readonly MessageTemplateText _descriprionRuPart;
    private readonly MessageTemplateText? _descriprionEnPart;
    private readonly string _player;
    private readonly ActionInfo? _actionInfo;
    private readonly bool _compatablePartners;
    private readonly Texts _texts;
    private readonly string _imagesfolder;
    private readonly string? _imagePath;
}