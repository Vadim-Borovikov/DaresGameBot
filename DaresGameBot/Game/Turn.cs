using System;
using System.Collections.Generic;
using System.Linq;
using AbstractBot.Models.MessageTemplates;
using DaresGameBot.Configs;
using DaresGameBot.Game.States;
using GryphonUtilities.Extensions;

namespace DaresGameBot.Game;

internal sealed class Turn
{
    public Turn(Texts texts, Func<string, string> getDisplayName, string tag, string description, string player,
        Arrangement arrangement) : this(texts, getDisplayName, tag, description, player.Yield(), arrangement)
    {}

    public Turn(Texts texts, Func<string, string> getDisplayName, string tag, string description,
        IEnumerable<string> players, Arrangement? arrangement = null)
    {
        _texts = texts;
        _getDisplayName = getDisplayName;
        _players = players;
        _arrangement = arrangement;
        _tagPart = new MessageTemplateText(tag);
        _descriprion = new MessageTemplateText(description);
    }

    public MessageTemplateText GetMessage()
    {
        MessageTemplateText message = _texts.TurnFormatFull;

        string playersPart = string.Join(_texts.DefaultSeparator, _players.Select(_getDisplayName));

        MessageTemplateText? partnersPart = null;
        if (_arrangement is not null)
        {
            partnersPart = GetPartnersPart(_texts, _arrangement);
        }

        return message.Format(_tagPart, playersPart, _descriprion, partnersPart);
    }

    public static MessageTemplateText GetPartnersPart(Texts texts, Arrangement arrangement,
        Func<string, string> getDisplayName)
    {
        string partnersPrefix = arrangement.Partners.Count > 1 ? texts.Partners : texts.Partner;
        string separator = arrangement.CompatablePartners ? texts.CompatablePartnersSeparator : texts.DefaultSeparator;
        string partnersText = string.Join(separator, arrangement.Partners.Select(getDisplayName));
        return texts.TurnPartnersFormat.Format(partnersPrefix, partnersText);
    }

    private MessageTemplateText GetPartnersPart(Texts texts, Arrangement arrangement)
    {
        return GetPartnersPart(texts, arrangement, _getDisplayName);
    }

    private readonly MessageTemplateText _tagPart;
    private readonly MessageTemplateText _descriprion;
    private readonly IEnumerable<string> _players;
    private readonly Arrangement? _arrangement;
    private readonly Texts _texts;
    private readonly Func<string, string> _getDisplayName;
}