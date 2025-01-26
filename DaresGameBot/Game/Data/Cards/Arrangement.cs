namespace DaresGameBot.Game.Data.Cards;

internal sealed class Arrangement
{
    public readonly byte Partners;
    public readonly bool CompatablePartners;
    public readonly byte Helpers;

    public Arrangement(byte partners, bool compatablePartners, byte helpers)
    {
        Partners = partners;
        CompatablePartners = compatablePartners;
        Helpers = helpers;
    }

    public override int GetHashCode() => System.HashCode.Combine(Partners, CompatablePartners, Helpers);
}