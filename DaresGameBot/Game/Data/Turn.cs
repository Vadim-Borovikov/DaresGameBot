using System.Collections.Generic;
using System.Linq;
using AbstractBot.Configs.MessageTemplates;

namespace DaresGameBot.Game.Data;

internal sealed class Turn
{
    public static MessageTemplateText Format = new();
    public static MessageTemplateText PartnerFormat = new();
    public static MessageTemplateText PartnersFormat = new();
    public static string Partner = "";
    public static string Partners = "";
    public static string PartnersSeparator = "";

    public Turn(string text, Player? player = null, List<Partner>? partners = null)
    {
        _messagePart = new MessageTemplateText(text);
        _player = player;
        _partners = partners;
    }

    public MessageTemplateText GetMessage(int playersAmount)
    {
        MessageTemplateText? partnerPart = null;
        if (_player is not null)
        {
            partnerPart = PartnerFormat.Format(_player.Name);
        }

        MessageTemplateText? partnersPart = null;
        if (_partners is not null && (_partners.Count != 0) && (_partners.Count != (playersAmount - 1)))
        {
            string partnersPrefix = _partners.Count > 1 ? Partners : Partner;
            string parnters = string.Join(PartnersSeparator, _partners.Select(p => p.ToString()));
            partnersPart = PartnersFormat.Format(partnersPrefix, parnters);
        }

        return Format.Format(_messagePart, partnerPart, partnersPart);
    }

    private readonly MessageTemplateText _messagePart;
    private readonly Player? _player;
    private readonly List<Partner>? _partners;
}