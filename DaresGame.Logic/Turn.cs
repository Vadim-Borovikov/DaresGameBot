using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaresGame.Logic
{
    public class Turn
    {
        internal string Text;
        internal List<Partner> Partners;

        public string GetMassage()
        {
            var builder = new StringBuilder();

            builder.Append(Text);

            if (Partners.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine();
                builder.Append(Partners.Count > 1 ? "Партнёры: " : "Партнёр: ");
                IEnumerable<string> parnters = Partners.Select(p => $"{p}");
                builder.Append(string.Join(", ", parnters));
            }

            return builder.ToString();
        }
    }
}