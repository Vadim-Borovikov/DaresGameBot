using System.Collections.Generic;
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
        public Settings Settings { get; set; }

        internal string Url => $"{Host}:{Port}/{Token}";

        [JsonProperty]
        public List<string> ManualLines { get; set; }

        [JsonProperty]
        public List<string> AdditionalCommandsLines { get; set; }
    }
}