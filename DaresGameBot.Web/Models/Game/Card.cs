namespace DaresGameBot.Web.Models.Game
{
    internal class Card
    {
        protected internal string Description { get; protected set; }
        protected internal ushort Players { get; protected set; }
        protected internal ushort PartnersToAssign { get; protected set; }
    }
}