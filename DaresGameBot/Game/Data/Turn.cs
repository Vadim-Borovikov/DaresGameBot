using System.Collections.Generic;
using System.Linq;
using AbstractBot.Configs.MessageTemplates;

namespace DaresGameBot.Game.Data;

internal sealed class Turn
{
    public static MessageTemplateText Format = new();
    public static MessageTemplateText PartnersFormat = new();
    public static string Partner = "";
    public static string Partners = "";
    public static string Helper = "";
    public static string Helpers = "";
    public static string PartnersSeparator = "";

    public Turn(string text, Player player, List<Player>? partners = null, List<Player>? helpers = null)
    {
        _messagePart = new MessageTemplateText(text);
        _player = player;
        _partners = partners;
        _helpers = helpers;
    }

    public MessageTemplateText GetMessage(int playersAmount)
    {
        MessageTemplateText? partnersPart = null;
        if (_partners is not null && (_partners.Count != 0) && (_partners.Count != (playersAmount - 1)))
        {
            string partnersPrefix = _partners.Count > 1 ? Partners : Partner;
            string parnters = string.Join(PartnersSeparator, _partners.Select(p => p.Name));
            partnersPart = PartnersFormat.Format(partnersPrefix, parnters);
        }

        MessageTemplateText? helpersPart = null;
        if (_helpers is not null && (_helpers.Count != 0))
        {
            string helpersPrefix = _helpers.Count > 1 ? Helpers : Helper;
            string helpers = string.Join(PartnersSeparator, _helpers.Select(p => p.Name));
            helpersPart = PartnersFormat.Format(helpersPrefix, helpers);
        }

        return Format.Format(_player.Name, _messagePart, partnersPart, helpersPart);
    }

    private readonly MessageTemplateText _messagePart;
    private readonly Player _player;
    private readonly List<Player>? _partners;
    private readonly List<Player>? _helpers;
}