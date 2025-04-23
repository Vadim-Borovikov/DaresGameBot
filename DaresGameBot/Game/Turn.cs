using System.Collections.Generic;
using System.IO;
using AbstractBot.Models.MessageTemplates;
using DaresGameBot.Configs;
using DaresGameBot.Game.Data;
using DaresGameBot.Game.States;
using GryphonUtilities.Extensions;

namespace DaresGameBot.Game;

internal sealed class Turn
{
    public Turn(Texts texts, bool includeEn, string imagesfolder, ActionData actionData, string player,
        Arrangement arrangement)
        : this(texts, includeEn, imagesfolder, actionData.Tag, actionData, player.Yield(), arrangement,
            actionData.ImagePath)
    {}

    public Turn(Texts texts, bool includeEn, string imagesfolder, string tag, CardData cardData,
        IEnumerable<string> players, Arrangement? arrangement = null, string? imagePath = null)
    {
        _texts = texts;
        _includeEn = includeEn;
        _players = players;
        _arrangement = arrangement;
        _tagPart = new MessageTemplateText(tag);
        _descriprionRuPart = new MessageTemplateText(cardData.Description);
        _descriprionEnPart = new MessageTemplateText(cardData.DescriptionEn);
        _imagePath = string.IsNullOrWhiteSpace(imagePath) ? null : Path.Combine(imagesfolder, imagePath);
    }

    public MessageTemplate GetMessage()
    {
        MessageTemplateText descriprionPart = _descriprionRuPart;
        if (_includeEn && _descriprionEnPart is not null)
        {
            descriprionPart = _texts.TurnDescriptionRuEnFormat.Format(_descriprionRuPart, _descriprionEnPart);
        }

        MessageTemplate message = _texts.TurnFormatFull;

        string playersPart = string.Join(_texts.DefaultSeparator, _players);

        MessageTemplateText? partnersPart = null;
        if (_arrangement is not null)
        {
            partnersPart = GetPartnersPart(_texts, _arrangement);
        }

        if (!string.IsNullOrWhiteSpace(_imagePath))
        {
            message = new MessageTemplateImage(message, _imagePath);
        }

        return message.Format(_tagPart, playersPart, descriprionPart, partnersPart);
    }

    public static MessageTemplateText GetPartnersPart(Texts texts, Arrangement arrangement)
    {
        string partnersPrefix = arrangement.Partners.Count > 1 ? texts.Partners : texts.Partner;
        string separator = arrangement.CompatablePartners ? texts.CompatablePartnersSeparator : texts.DefaultSeparator;
        string partnersText = string.Join(separator, arrangement.Partners);
        return texts.TurnPartnersFormat.Format(partnersPrefix, partnersText);
    }

    private readonly MessageTemplateText _tagPart;
    private readonly MessageTemplateText _descriprionRuPart;
    private readonly MessageTemplateText? _descriprionEnPart;
    private readonly IEnumerable<string> _players;
    private readonly Arrangement? _arrangement;
    private readonly Texts _texts;
    private readonly bool _includeEn;
    private readonly string? _imagePath;
}