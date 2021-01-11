using System.Collections.Generic;
using DaresGameBot.Logic;

namespace DaresGameBot.Web.Models
{
    public sealed class Settings
    {
        internal readonly ushort InitialPlayersAmount;
        internal readonly float InitialChoiceChance;
        internal readonly IReadOnlyCollection<Deck> Decks;

        internal Settings(ushort initialPlayersAmount, float initialChoiceChance, IReadOnlyCollection<Deck> decks)
        {
            InitialPlayersAmount = initialPlayersAmount;
            InitialChoiceChance = initialChoiceChance;
            Decks = decks;
        }
    }
}