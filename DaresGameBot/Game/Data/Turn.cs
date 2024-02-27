using System.Collections.Generic;
using System.IO;
using System.Linq;
using AbstractBot.Configs.MessageTemplates;
using DaresGameBot.Configs;

namespace DaresGameBot.Game.Data;

internal sealed class Turn
{
    public Turn(Texts texts, string imagesfolder, string tag, string descriprion, CompanionsInfo? companions = null,
        string? imagePath = null)
    {
        _texts = texts;
        _imagesfolder = imagesfolder;
        _companions = companions;
        _tagPart = new MessageTemplateText(tag);
        _descriprionPart = new MessageTemplateText(descriprion);
        _imagePath = imagePath;
    }

    public MessageTemplate GetMessage(int playersAmount)
    {
        MessageTemplateText? partnersPart = null;
        IReadOnlyList<Player>? partners = _companions?.Partners;
        if (partners is not null && (partners.Count != 0) && (partners.Count != (playersAmount - 1)))
        {
            string partnersPrefix = partners.Count > 1 ? _texts.Partners : _texts.Partner;
            string partnersText = string.Join(_texts.PartnersSeparator, partners.Select(p => p.Name));
            partnersPart = _texts.TurnPartnersFormat.Format(partnersPrefix, partnersText);
        }

        MessageTemplateText? helpersPart = null;
        IReadOnlyList<Player>? helpers = _companions?.Helpers;
        if (helpers is not null && (helpers.Count != 0))
        {
            string helpersPrefix = helpers.Count > 1 ? _texts.Helpers : _texts.Helper;
            string helpersText = string.Join(_texts.PartnersSeparator, helpers.Select(p => p.Name));
            helpersPart = _texts.TurnPartnersFormat.Format(helpersPrefix, helpersText);
        }

        MessageTemplate message = _texts.TurnFormat;
        if (!string.IsNullOrWhiteSpace(_imagePath))
        {
            string path = Path.Combine(_imagesfolder, _imagePath);
            message = new MessageTemplateImage(message, path);
        }

        return message.Format(_companions?.Player.Name, _tagPart, _descriprionPart, partnersPart, helpersPart);
    }

    private readonly MessageTemplateText _tagPart;
    private readonly MessageTemplateText _descriprionPart;
    private readonly Texts _texts;
    private readonly string _imagesfolder;
    private readonly CompanionsInfo? _companions;
    private readonly string? _imagePath;
}