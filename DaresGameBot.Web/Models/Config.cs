using Newtonsoft.Json;

namespace DaresGameBot.Web.Models
{
    public sealed class Config : Bot.Config
    {
        [JsonProperty]
        public string GoogleCredentialJson { get; set; }
    }
}