using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaresGameBot.Logic
{
    public sealed class Turn
    {
        private readonly string _text;
        private readonly List<Partner> _partners;

        internal Turn(string text, List<Partner> partners)
        {
            _text = text;
            _partners = partners;
        }

        public string GetMessage(int playersAmount)
        {
            if ((_partners.Count == 0) || (_partners.Count == (playersAmount - 1)))
            {
                return _text;
            }

            var builder = new StringBuilder(_text);

            builder.AppendLine();
            builder.AppendLine();
            builder.Append(_partners.Count > 1 ? "Партнёры: " : "Партнёр: ");
            IEnumerable<string> parnters = _partners.Select(p => $"{p}");
            builder.Append(string.Join(", ", parnters));

            return builder.ToString();
        }
    }
}