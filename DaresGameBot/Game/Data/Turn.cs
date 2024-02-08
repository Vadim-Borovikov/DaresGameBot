using System.Collections.Generic;
using System.IO;
using System.Linq;
using AbstractBot.Configs.MessageTemplates;
using DaresGameBot.Configs;

namespace DaresGameBot.Game.Data;

internal sealed class Turn
{
    public Turn(Config config, string tag, string descriprion, Player? player = null, string? imagePath = null,
        List<Player>? partners = null, List<Player>? helpers = null)
    {
        _tagPart = new MessageTemplateText(tag);
        _descriprionPart = new MessageTemplateText(descriprion);
        _config = config;
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
            string partnersPrefix = _partners.Count > 1 ? _config.Texts.Partners : _config.Texts.Partner;
            string parnters = string.Join(_config.Texts.PartnersSeparator, _partners.Select(p => p.Name));
            partnersPart = _config.Texts.TurnPartnersFormat.Format(partnersPrefix, parnters);
        }

        MessageTemplateText? helpersPart = null;
        if (_helpers is not null && (_helpers.Count != 0))
        {
            string helpersPrefix = _helpers.Count > 1 ? _config.Texts.Helpers : _config.Texts.Helper;
            string helpers = string.Join(_config.Texts.PartnersSeparator, _helpers.Select(p => p.Name));
            helpersPart = _config.Texts.TurnPartnersFormat.Format(helpersPrefix, helpers);
        }

        MessageTemplate message = _config.Texts.TurnFormat;
        if (!string.IsNullOrWhiteSpace(_imagePath))
        {
            string path = Path.Combine(_config.ImagesFolder, _imagePath);
            message = new MessageTemplateImage(message, path);
        }

        return message.Format(_player?.Name, _tagPart, _descriprionPart, partnersPart, helpersPart);
    }

    private readonly MessageTemplateText _tagPart;
    private readonly MessageTemplateText _descriprionPart;
    private readonly Config _config;
    private readonly Player? _player;
    private readonly string? _imagePath;
    private readonly List<Player>? _partners;
    private readonly List<Player>? _helpers;
}