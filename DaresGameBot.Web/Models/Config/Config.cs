using System.Collections.Generic;
using DaresGameBot.Logic;
using Newtonsoft.Json;

namespace DaresGameBot.Web.Models.Config
{
    public sealed class Config
    {
        [JsonProperty]
        public string Token { get; set; }

        [JsonProperty]
        public string Host { get; set; }

        [JsonProperty]
        public int Port { get; set; }

        [JsonProperty]
        public ushort InitialPlayersAmount { get; set; }

        [JsonProperty]
        public float InitialChoiceChance { get; set; }

        [JsonProperty]
        public string DecksJson { get; set; }

        internal List<Deck> Decks => JsonConvert.DeserializeObject<List<Deck>>(DecksJson);

        internal string Url => $"{Host}:{Port}/{Token}";

        [JsonProperty]
        public List<string> ManualLines { get; set; }

        [JsonProperty]
        public List<string> AdditionalCommandsLines { get; set; }
    }
}