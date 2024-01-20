using System.Collections.Generic;
using System.Linq;
using AbstractBot.Configs.MessageTemplates;
using AbstractBot.Extensions;

namespace DaresGameBot.Game.Data;

internal sealed class Turn
{
    public static MessageTemplateText TurnFormat = new();
    public static string Partner = "";
    public static string Partners = "";
    public static string PartnersSeparator = "";

    public Turn(string text, List<Partner>? partners = null)
    {
        _messageTemplate = new MessageTemplateText(text);
        _partners = partners;
    }

    public MessageTemplateText GetMessage(ushort playersAmount)
    {
        if (_partners is null || (_partners.Count == 0) || (_partners.Count == (playersAmount - 1)))
        {
            return _messageTemplate;
        }

        string partnersPrefix = _partners.Count > 1 ? Partners : Partner;
        string parnters = string.Join(PartnersSeparator, _partners.Select(p => p.ToString()));
        return TurnFormat.Format(_messageTemplate, partnersPrefix.Escape(), parnters.Escape());
    }

    private readonly MessageTemplateText _messageTemplate;
    private readonly List<Partner>? _partners;
}