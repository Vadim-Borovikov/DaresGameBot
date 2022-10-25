namespace DaresGameBot.Web.Models;

public sealed class BotSingleton : IDisposable
{
    internal readonly Bot Bot;

    public BotSingleton(Config config) => Bot = new Bot(config);

    public void Dispose() => Bot.Dispose();
}