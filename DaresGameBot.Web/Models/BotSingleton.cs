using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DaresGameBot.Web.Models;

public sealed class BotSingleton : IDisposable
{
    internal readonly Bot.Bot Bot;

    public BotSingleton(IOptions<Config> options)
    {
        Config config = options.Value;

        if (config.GoogleCredential is null || (config.GoogleCredential.Count == 0))
        {
            string json = config.GoogleCredentialJson
                          ?? throw new NullReferenceException(nameof(config.GoogleCredentialJson));

            config.GoogleCredential = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }
        Bot = new Bot.Bot(config);
    }

    public void Dispose() => Bot.Dispose();
}
