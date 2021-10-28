using AbstractBot;
using Newtonsoft.Json;

namespace DaresGameBot.Bot
{
    public class BotConfig : ConfigGoogleSheets
    {
        [JsonProperty]
        public ushort InitialPlayersAmount { get; set; }

        [JsonProperty]
        public float InitialChoiceChance { get; set; }

        [JsonProperty]
        public string ActionsGoogleRange { get; set; }

        [JsonProperty]
        public string QuestionsGoogleRange { get; set; }
    }
}