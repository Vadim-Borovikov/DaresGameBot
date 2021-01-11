using Newtonsoft.Json;

namespace DaresGameBot.Web.Models.Config
{
    public sealed class Settings
    {
        [JsonProperty]
        public ushort InitialPlayersAmount { get; set; }

        [JsonProperty]
        public float InitialChoiceChance { get; set; }

        [JsonProperty]
        public string DecksJson { get; set; }
    }
}