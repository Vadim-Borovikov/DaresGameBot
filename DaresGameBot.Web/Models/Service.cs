using System.Threading;
using System.Threading.Tasks;
using GoogleSheetsManager;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace DaresGameBot.Web.Models
{
    internal sealed class Service : IHostedService
    {
        public Service(IBot bot) => _bot = bot;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            string googleCredentialsJson = _bot.Config.GoogleCredentialsJson;
            if (string.IsNullOrWhiteSpace(googleCredentialsJson))
            {
                googleCredentialsJson = JsonConvert.SerializeObject(_bot.Config.GoogleCredentials);
            }
            _googleSheetsProvider = new Provider(googleCredentialsJson, ApplicationName, _bot.Config.GoogleSheetId);
            _bot.Initialize(_googleSheetsProvider);

            return _bot.Client.SetWebhookAsync(_bot.Config.Url, cancellationToken: cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _googleSheetsProvider.Dispose();
            return _bot.Client.DeleteWebhookAsync(cancellationToken);
        }

        private readonly IBot _bot;
        private Provider _googleSheetsProvider;

        private const string ApplicationName = "DaresGameBot";
    }
}