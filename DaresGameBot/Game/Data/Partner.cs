using System;

namespace DaresGameBot.Game.Data;

internal sealed class Partner : IComparable<Partner>
{
    public static string Choosable = "";

    public Partner(byte? partnerNumber = null) => _number = partnerNumber;

    public int CompareTo(Partner? other)
    {
        if (other?._number is null)
        {
            return _number.HasValue ? -1 : 0;
        }

        return _number?.CompareTo(other._number.Value) ?? 1;
    }

    public override string ToString() => _number?.ToString() ?? Choosable;

    private readonly byte? _number;
}