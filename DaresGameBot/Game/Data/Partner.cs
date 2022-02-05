using System;

namespace DaresGameBot.Game.Data;

internal sealed class Partner : IComparable<Partner>
{
    private readonly ushort? _number;

    public Partner(ushort partnerNumber) => _number = partnerNumber;

    public Partner() => _number = null;

    public int CompareTo(Partner? other)
    {
        if (other?._number is null)
        {
            return _number.HasValue ? -1 : 0;
        }

        return _number?.CompareTo(other._number.Value) ?? 1;
    }

    public override string ToString() => _number?.ToString() ?? "🤩";
}
