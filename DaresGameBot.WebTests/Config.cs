using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace DaresGameBot.WebTests;

internal sealed class Config
{
    [JsonProperty]
    public int ApiId { get; set; }

    [JsonProperty]
    public string ApiHash { get; set; } = null!;

    [JsonProperty]
    public string PhoneNumber { get; set; } = null!;

    [JsonProperty]
    public string BotUsernameTest { get; set; } = null!;

    [JsonProperty]
    public string BotUsernameProd { get; set; } = null!;

    [JsonProperty]
    public string New { get; set; } = null!;

    [JsonProperty]
    public string Rates { get; set; } = null!;

    [JsonProperty]
    public List<string> PlayersList { get; set; } = null!;

    public string Players => string.Join(Environment.NewLine, PlayersList);

    [JsonProperty]
    public string FirstMessageStart { get; set; } = null!;

    [JsonProperty]
    public string SecondMessageStart { get; set; } = null!;

    [JsonProperty]
    public string HostLocal { get; set; } = null!;

    [JsonProperty]
    public string HostAzure { get; set; } = null!;

    public Uri UriLocal => new(HostLocal);
    public Uri UriAzure => new(HostAzure);

    public static Config Load()
    {
        ConfigurationBuilder builder = new();
        return builder.AddJsonFile("appsettings.json")
                      .Build()
                      .Get<Config>()!;
    }

    public string? Get(string what)
    {
        return what switch
        {
            "api_id"       => ApiId.ToString(),
            "api_hash"     => ApiHash,
            "phone_number" => PhoneNumber,
            _              => null
        };
    }
}