using Newtonsoft.Json;

namespace DaresGameBot.Web
{
    public sealed class Config : Bot.Config
    {
        [JsonProperty]
        public string GoogleCredentialJson { get; set; }
    }
}