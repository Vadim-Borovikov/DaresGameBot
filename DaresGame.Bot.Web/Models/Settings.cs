using System.Collections.Generic;
using DaresGame.Logic;
using Newtonsoft.Json;

namespace DaresGame.Bot.Web.Models
{
    public class Settings
    {
        internal readonly int InitialPlayersAmount;
        internal readonly float InitialChoiceChance;
        internal readonly List<Deck> Decks;

        internal Settings(int initialPlayersAmount, float initialChoiceChance, string decksJson)
        {
            InitialPlayersAmount = initialPlayersAmount;
            InitialChoiceChance = initialChoiceChance;
            Decks = JsonConvert.DeserializeObject<List<Deck>>(decksJson);
        }
    }
}