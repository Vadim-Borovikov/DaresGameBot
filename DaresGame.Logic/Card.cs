namespace DaresGame.Logic
{
    public class Card
    {
        internal readonly string Description;
        internal readonly int PartnersAmount;

        public Card(string description, int partnersAmount)
        {
            Description = description;
            PartnersAmount = partnersAmount;
        }
    }
}