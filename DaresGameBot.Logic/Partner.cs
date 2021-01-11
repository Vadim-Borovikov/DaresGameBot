﻿using System;

namespace DaresGameBot.Logic
{
    public sealed class Partner : IComparable<Partner>
    {
        private readonly ushort? _number;

        internal Partner(ushort partnerNumber) => _number = partnerNumber;

        internal Partner() => _number = null;

        public int CompareTo(Partner other)
        {
            if (!_number.HasValue)
            {
                return 1;
            }

            if (!other._number.HasValue)
            {
                return -1;
            }

            return _number.Value.CompareTo(other._number.Value);
        }

        public override string ToString() => _number?.ToString() ?? "🤩";
    }
}