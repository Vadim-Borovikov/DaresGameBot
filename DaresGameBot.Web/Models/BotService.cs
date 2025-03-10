namespace DaresGameBot.Web.Models;

public sealed class BotService : IHostedService, IDisposable
{
    internal readonly Bot Bot;

    public BotService(Bot bot) => Bot = bot;

    public void Dispose() => Bot.Dispose();

    public Task StartAsync(CancellationToken cancellationToken) => Bot.StartAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Bot.StopAsync(cancellationToken);
}