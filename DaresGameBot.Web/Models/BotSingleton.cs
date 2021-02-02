using System;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DaresGameBot.Web.Models
{
    public sealed class BotSingleton : IDisposable
    {
        internal readonly Bot.Bot Bot;

        public BotSingleton(IOptions<Config> options)
        {
            Config config = options.Value;

            Bot.Config botConfig = config.BotConfig ?? JsonConvert.DeserializeObject<Bot.Config>(config.BotConfigJson);

            string googleCredentialsJson = config.GoogleCredentialsJson;
            if (string.IsNullOrWhiteSpace(googleCredentialsJson))
            {
                googleCredentialsJson = JsonConvert.SerializeObject(config.GoogleCredentials);
            }
            Bot = new Bot.Bot(botConfig, googleCredentialsJson);
        }

        public void Dispose() => Bot.Dispose();
    }
}