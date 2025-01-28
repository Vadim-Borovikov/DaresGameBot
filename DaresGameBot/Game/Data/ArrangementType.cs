using System;

namespace DaresGameBot.Game.Data;

internal readonly struct ArrangementType : IEquatable<ArrangementType>
{
    public readonly byte Partners;
    public readonly bool CompatablePartners;

    public ArrangementType(byte partners, bool compatablePartners)
    {
        Partners = partners;
        CompatablePartners = compatablePartners;
    }

    public bool Equals(ArrangementType other)
    {
        return (Partners == other.Partners) && (CompatablePartners == other.CompatablePartners);
    }

    public static bool operator ==(ArrangementType left, ArrangementType right) => left.Equals(right);
    public static bool operator !=(ArrangementType left, ArrangementType right) => !left.Equals(right);

    public override bool Equals(object? obj) => obj is ArrangementType other && Equals(other);

    public override int GetHashCode() => CompatablePartners ? -Partners : Partners;
}