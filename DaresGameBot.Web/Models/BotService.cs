using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace DaresGameBot.Web.Models
{
    internal sealed class BotService : IHostedService
    {
        public BotService(BotSingleton singleton) => _bot = singleton.Bot;

        public Task StartAsync(CancellationToken cancellationToken) => _bot.StartAsync(cancellationToken);

        public Task StopAsync(CancellationToken cancellationToken) => _bot.StopAsync(cancellationToken);

        private readonly Bot.Bot _bot;
    }
}