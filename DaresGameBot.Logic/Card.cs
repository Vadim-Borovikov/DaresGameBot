namespace DaresGameBot.Logic
{
    public sealed class Card
    {
        public string Description { internal get; set; }
        public int Players { internal get; set; }
        public int PartnersToAssign { internal get; set; }
    }
}