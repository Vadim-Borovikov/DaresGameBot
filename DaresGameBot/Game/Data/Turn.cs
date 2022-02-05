using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaresGameBot.Game.Data;

internal sealed class Turn
{
    private readonly string _text;
    private readonly List<Partner>? _partners;

    public Turn(string text, List<Partner>? partners = null)
    {
        _text = text;
        _partners = partners;
    }

    public string GetMessage(ushort playersAmount)
    {
        if (_partners is null || (_partners.Count == 0) || (_partners.Count == (playersAmount - 1)))
        {
            return _text;
        }

        StringBuilder builder = new(_text);

        builder.AppendLine();
        builder.AppendLine();
        builder.Append(_partners.Count > 1 ? "Партнёры: " : "Партнёр: ");
        IEnumerable<string> parnters = _partners.Select(p => $"{p}");
        builder.Append(string.Join(", ", parnters));

        return builder.ToString();
    }
}
