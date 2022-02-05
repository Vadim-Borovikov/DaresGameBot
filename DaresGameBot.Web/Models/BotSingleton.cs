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
            if (string.IsNullOrWhiteSpace(config.GoogleCredentialJson))
            {
                throw new NullReferenceException(nameof(config.GoogleCredentialJson));
            }

            config.GoogleCredential =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(config.GoogleCredentialJson);
        }
        Bot = new Bot.Bot(config);
    }

    public void Dispose() => Bot.Dispose();
}
