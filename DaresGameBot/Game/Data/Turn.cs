using System.Collections.Generic;
using System.IO;
using AbstractBot.Configs.MessageTemplates;
using DaresGameBot.Configs;

namespace DaresGameBot.Game.Data;

internal sealed class Turn
{
    public Turn(Texts texts, string imagesfolder, string tag, string? prefix, string descriprionRu,
        string? descriptionEn, CompanionsInfo? companions = null, string? imagePath = null)
    {
        _texts = texts;
        _imagesfolder = imagesfolder;
        _prefixPart = prefix is null ? null : new MessageTemplateText(prefix);
        _companions = companions;
        _tagPart = new MessageTemplateText(tag);
        _descriprionRuPart = new MessageTemplateText(descriprionRu);
        _descriprionEnPart = descriptionEn is null ? null : new MessageTemplateText(descriptionEn);
        _imagePath = imagePath;
    }

    public MessageTemplate GetMessage(int playersAmount, bool includeEn = false)
    {
        MessageTemplateText descriprionPart = _descriprionRuPart;
        if (includeEn && _descriprionEnPart is not null)
        {
            descriprionPart = _texts.TurnDescriptionRuEnFormat.Format(_descriprionRuPart, _descriprionEnPart);
        }

        MessageTemplateText? partnersPart = null;
        IReadOnlyList<string>? partners = _companions?.Partners;
        if (partners is not null && (partners.Count != 0) && (partners.Count != (playersAmount - 1)))
        {
            string partnersPrefix = partners.Count > 1 ? _texts.Partners : _texts.Partner;
            string partnersText = string.Join(_texts.PartnersSeparator, partners);
            partnersPart = _texts.TurnPartnersFormat.Format(partnersPrefix, partnersText);
        }

        MessageTemplateText? helpersPart = null;
        IReadOnlyList<string>? helpers = _companions?.Helpers;
        if (helpers is not null && (helpers.Count != 0))
        {
            string helpersPrefix = helpers.Count > 1 ? _texts.Helpers : _texts.Helper;
            string helpersText = string.Join(_texts.PartnersSeparator, helpers);
            helpersPart = _texts.TurnPartnersFormat.Format(helpersPrefix, helpersText);
        }

        MessageTemplate message = _texts.TurnFormat;
        if (!string.IsNullOrWhiteSpace(_imagePath))
        {
            string path = Path.Combine(_imagesfolder, _imagePath);
            message = new MessageTemplateImage(message, path);
        }

        return message.Format(_tagPart, _companions?.Player, _prefixPart, descriprionPart, partnersPart, helpersPart);
    }

    private readonly MessageTemplateText _tagPart;
    private readonly MessageTemplateText _descriprionRuPart;
    private readonly MessageTemplateText? _descriprionEnPart;
    private readonly Texts _texts;
    private readonly string _imagesfolder;
    private readonly MessageTemplateText? _prefixPart;
    private readonly CompanionsInfo? _companions;
    private readonly string? _imagePath;
}