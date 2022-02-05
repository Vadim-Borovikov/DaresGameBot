using Newtonsoft.Json;

namespace DaresGameBot.Web.Models;

public sealed class Config : Bot.BotConfig
{
    [JsonProperty]
    public string? GoogleCredentialJson { get; set; }
}