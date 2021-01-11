namespace DaresGameBot.Logic
{
    public class Card
    {
        public string Description { internal get; set; }
        public ushort Players { internal get; set; }
        public ushort PartnersToAssign { internal get; set; }
    }
}