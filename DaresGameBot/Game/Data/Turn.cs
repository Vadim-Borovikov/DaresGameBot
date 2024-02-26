using System.Collections.Generic;
using System.IO;
using System.Linq;
using AbstractBot.Configs.MessageTemplates;
using DaresGameBot.Configs;

namespace DaresGameBot.Game.Data;

internal sealed class Turn
{
    public Turn(Texts texts, string imagesfolder, string tag, string descriprion, Player? player = null,
        string? imagePath = null, List<Player>? partners = null, List<Player>? helpers = null)
    {
        _texts = texts;
        _imagesfolder = imagesfolder;
        _tagPart = new MessageTemplateText(tag);
        _descriprionPart = new MessageTemplateText(descriprion);
        _player = player;
        _imagePath = imagePath;
        _partners = partners;
        _helpers = helpers;
    }

    public MessageTemplate GetMessage(int playersAmount)
    {
        MessageTemplateText? partnersPart = null;
        if (_partners is not null && (_partners.Count != 0) && (_partners.Count != (playersAmount - 1)))
        {
            string partnersPrefix = _partners.Count > 1 ? _texts.Partners : _texts.Partner;
            string parnters = string.Join(_texts.PartnersSeparator, _partners.Select(p => p.Name));
            partnersPart = _texts.TurnPartnersFormat.Format(partnersPrefix, parnters);
        }

        MessageTemplateText? helpersPart = null;
        if (_helpers is not null && (_helpers.Count != 0))
        {
            string helpersPrefix = _helpers.Count > 1 ? _texts.Helpers : _texts.Helper;
            string helpers = string.Join(_texts.PartnersSeparator, _helpers.Select(p => p.Name));
            helpersPart = _texts.TurnPartnersFormat.Format(helpersPrefix, helpers);
        }

        MessageTemplate message = _texts.TurnFormat;
        if (!string.IsNullOrWhiteSpace(_imagePath))
        {
            string path = Path.Combine(_imagesfolder, _imagePath);
            message = new MessageTemplateImage(message, path);
        }

        return message.Format(_player?.Name, _tagPart, _descriprionPart, partnersPart, helpersPart);
    }

    private readonly MessageTemplateText _tagPart;
    private readonly MessageTemplateText _descriprionPart;
    private readonly Texts _texts;
    private readonly string _imagesfolder;
    private readonly Player? _player;
    private readonly string? _imagePath;
    private readonly List<Player>? _partners;
    private readonly List<Player>? _helpers;
}