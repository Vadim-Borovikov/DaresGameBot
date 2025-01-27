namespace DaresGameBot.Game.Data.Cards;

internal sealed class Arrangement
{
    public readonly byte Partners;
    public readonly bool CompatablePartners;

    public Arrangement(byte partners, bool compatablePartners)
    {
        Partners = partners;
        CompatablePartners = compatablePartners;
    }

    public override int GetHashCode() => CompatablePartners ? -Partners : Partners;
}