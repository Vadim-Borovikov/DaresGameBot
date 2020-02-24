using System;

namespace DaresGame.Logic
{
    public class Partner : IComparable<Partner>
    {
        private readonly bool _byChoice;
        private readonly int _partnerNumber;

        internal Partner(int partnerNumber)
        {
            _byChoice = false;
            _partnerNumber = partnerNumber;
        }

        internal Partner(bool byChoice)
        {
            _byChoice = byChoice;
            _partnerNumber = int.MaxValue;
        }

        public int CompareTo(Partner other) => _partnerNumber.CompareTo(other._partnerNumber);

        public override string ToString() => _byChoice ? "🤩" : $"{_partnerNumber}";
    }
}