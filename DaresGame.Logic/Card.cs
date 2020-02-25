namespace DaresGame.Logic
{
    public class Card
    {
        internal readonly string Description;
        internal readonly int Players;
        internal readonly int PartnersToAssign;

        public Card(string description, int players, int partnersToAssign)
        {
            Description = description;
            Players = players;
            PartnersToAssign = partnersToAssign;
        }
    }
}