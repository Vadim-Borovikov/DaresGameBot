using System.Collections.Generic;
using Newtonsoft.Json;

namespace DaresGameBot.Web
{
    public sealed class Config
    {
        [JsonProperty]
        public Bot.Config BotConfig { get; set; }

        [JsonProperty]
        public Dictionary<string, string> GoogleCredentials { get; set; }

        [JsonProperty]
        public string BotConfigJson { get; set; }

        [JsonProperty]
        public string GoogleCredentialsJson { get; set; }
    }
}