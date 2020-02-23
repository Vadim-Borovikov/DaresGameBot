using System;

namespace DaresGame.Logic
{
    public class Partner : IComparable<Partner>
    {
        public readonly bool ByChoice;
        public readonly int PartnerNumber;

        internal Partner(int partnerNumber)
        {
            ByChoice = false;
            PartnerNumber = partnerNumber;
        }

        internal Partner(bool byChoice)
        {
            ByChoice = byChoice;
            PartnerNumber = int.MaxValue;
        }

        public int CompareTo(Partner other) => PartnerNumber.CompareTo(other.PartnerNumber);
    }
}