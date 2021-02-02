namespace DaresGameBot.Web.Models.Game
{
    internal class Card
    {
        public string Description { get; protected set; }
        public ushort Players { get; protected set; }
        public ushort PartnersToAssign { get; protected set; }
    }
}