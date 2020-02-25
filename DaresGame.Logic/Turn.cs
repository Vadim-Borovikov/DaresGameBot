using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaresGame.Logic
{
    public class Turn
    {
        private readonly string _text;
        private readonly List<Partner> _partners;

        internal Turn(string text, List<Partner> partners)
        {
            _text = text;
            _partners = partners;
        }

        public override string ToString()
        {
            var builder = new StringBuilder(_text);

            if (_partners.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine();
                builder.Append(_partners.Count > 1 ? "Партнёры: " : "Партнёр: ");
                IEnumerable<string> parnters = _partners.Select(p => $"{p}");
                builder.Append(string.Join(", ", parnters));
            }

            return builder.ToString();
        }
    }
}