using System.IO;
using AbstractBot.Configs.MessageTemplates;
using DaresGameBot.Configs;
using DaresGameBot.Game.Data;

namespace DaresGameBot.Game;

internal sealed class Turn
{
    public Turn(Texts texts, string imagesfolder, ActionData actionData, string player, Arrangement arrangement)
        : this(texts, imagesfolder, actionData.Tag, actionData, player, arrangement, actionData.ImagePath)
    {}

    public Turn(Texts texts, string imagesfolder, string tag, CardData cardData, string player,
        Arrangement? arrangement = null, string? imagePath = null)
    {
        _texts = texts;
        _player = player;
        _arrangement = arrangement;
        _tagPart = new MessageTemplateText(tag);
        _descriprionRuPart = new MessageTemplateText(cardData.Description);
        _descriprionEnPart = new MessageTemplateText(cardData.DescriptionEn);
        _imagePath = string.IsNullOrWhiteSpace(imagePath) ? null : Path.Combine(imagesfolder, imagePath);
    }

    public MessageTemplate GetMessage(bool includeEn = false)
    {
        MessageTemplateText descriprionPart = _descriprionRuPart;
        if (includeEn && _descriprionEnPart is not null)
        {
            descriprionPart = _texts.TurnDescriptionRuEnFormat.Format(_descriprionRuPart, _descriprionEnPart);
        }

        MessageTemplate message = _texts.TurnFormatFull;

        MessageTemplateText? partnersPart = null;
        if (_arrangement is not null)
        {
            partnersPart = GetPartnersPart(_texts, _arrangement);
        }

        if (!string.IsNullOrWhiteSpace(_imagePath))
        {
            message = new MessageTemplateImage(message, _imagePath);
        }

        return message.Format(_tagPart, _player, descriprionPart, partnersPart);
    }

    public static MessageTemplateText GetPartnersPart(Texts texts, Arrangement arrangement)
    {
        string partnersPrefix = arrangement.Partners.Count > 1 ? texts.Partners : texts.Partner;
        string separator =
            arrangement.CompatablePartners ? texts.CompatablePartnersSeparator : texts.PartnersSeparator;
        string partnersText = string.Join(separator, arrangement.Partners);
        return texts.TurnPartnersFormat.Format(partnersPrefix, partnersText);
    }

    private readonly MessageTemplateText _tagPart;
    private readonly MessageTemplateText _descriprionRuPart;
    private readonly MessageTemplateText? _descriprionEnPart;
    private readonly string _player;
    private readonly Arrangement? _arrangement;
    private readonly Texts _texts;
    private readonly string? _imagePath;
}