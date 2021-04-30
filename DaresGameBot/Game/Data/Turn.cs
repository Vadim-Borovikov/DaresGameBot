using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaresGameBot.Game.Data
{
    internal sealed class Turn
    {
        public ushort Rejects { get; private set; }

        private string Text => $"{Card.Tag} {Card.Description}";
        public readonly Card Card;
        private List<Partner> _partners;
        public readonly HashSet<ushort> MarkedPartners = new HashSet<ushort>();

        public Turn(Card card, List<Partner> partners, ushort rejects)
        {
            Card = card;
            _partners = partners;
            Rejects = rejects;
            MarkPartners();
        }

        private void MarkPartners()
        {
            foreach (ushort number in _partners.Where(p => p.Number.HasValue).Select(p => p.Number.Value))
            {
                MarkedPartners.Add(number);
            }
        }

        public string GetMessage()
        {
            if ((_partners.Count == 0) || (_partners.Count == (Card.PartnersToAssign - 1)))
            {
                return Text;
            }

            var builder = new StringBuilder(Text);

            builder.AppendLine();
            builder.AppendLine();
            builder.Append(_partners.Count > 1 ? "Партнёры: " : "Партнёр: ");
            IEnumerable<string> parnters = _partners.Select(p => $"{p}");
            builder.Append(string.Join(", ", parnters));

            return builder.ToString();
        }

        public void Reject(List<Partner> newPartners)
        {
            --Rejects;
            _partners = newPartners;
            MarkPartners();
        }
    }
}