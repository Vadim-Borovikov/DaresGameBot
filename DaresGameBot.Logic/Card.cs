namespace DaresGameBot.Logic
{
    public sealed class Card
    {
        public string Description { internal get; set; }
        public ushort Players { internal get; set; }
        public ushort PartnersToAssign { internal get; set; }
    }
}