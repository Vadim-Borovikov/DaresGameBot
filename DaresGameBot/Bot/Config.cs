using System.Collections.Generic;
using Newtonsoft.Json;

namespace DaresGameBot.Bot
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

        internal string Url => $"{Host}:{Port}/{Token}";

        [JsonProperty]
        public string GoogleSheetId { get; set; }

        [JsonProperty]
        public string GoogleRange { get; set; }

        [JsonProperty]
        public List<string> ManualLines { get; set; }

        [JsonProperty]
        public List<string> AdditionalCommandsLines { get; set; }

        [JsonProperty]
        public string DontUnderstandStickerFileId { get; set; }
    }
}