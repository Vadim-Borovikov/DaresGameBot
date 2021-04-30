using System;

namespace DaresGameBot.Game.Data
{
    internal sealed class Partner : IComparable<Partner>
    {
        public readonly ushort? Number;

        public Partner(ushort partnerNumber) => Number = partnerNumber;

        public Partner() => Number = null;

        public int CompareTo(Partner other)
        {
            if (!Number.HasValue)
            {
                return 1;
            }

            if (!other.Number.HasValue)
            {
                return -1;
            }

            return Number.Value.CompareTo(other.Number.Value);
        }

        public override string ToString() => Number?.ToString() ?? "🤩";
    }
}