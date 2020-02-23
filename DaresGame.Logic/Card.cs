namespace DaresGame.Logic
{
    public class Card
    {
        internal readonly string Description;
        internal readonly int PartnersNumber;

        public Card(string description, int partnersNumber)
        {
            Description = description;
            PartnersNumber = partnersNumber;
        }
    }
}