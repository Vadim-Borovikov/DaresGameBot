using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace DaresGameBot.Web.Models
{
    internal class Service : IHostedService
    {
        public Service(IBot bot)
        {
            _bot = bot;
            _bot.InitCommands();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _bot.Client.SetWebhookAsync(_bot.Config.Url, cancellationToken: cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _bot.Client.DeleteWebhookAsync(cancellationToken);
        }

        private readonly IBot _bot;
    }
}