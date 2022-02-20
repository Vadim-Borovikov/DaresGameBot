using Microsoft.Extensions.Options;

namespace DaresGameBot.Web.Models;

public sealed class BotSingleton : IDisposable
{
    internal readonly Bot Bot;

    public BotSingleton(IOptions<ConfigJson> options)
    {
        ConfigJson configJson = options.Value;
        Config config = configJson.Convert();
        Bot = new Bot(config);
    }

    public void Dispose() => Bot.Dispose();
}
