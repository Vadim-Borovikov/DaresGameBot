using System.Globalization;
using DaresGameBot.Web.Models;
using GryphonUtilities;
using GryphonUtilities.Time;
using Microsoft.Extensions.Options;

namespace DaresGameBot.Web;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        Logger.DeleteExceptionLog();
        Clock clock = new();
        Logger logger = new(clock);
        try
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            Config config = Configure(builder) ?? throw new NullReferenceException("Can't load config.");
            clock = new Clock(config.SystemTimeZoneIdLogs);
            logger = new Logger(clock);
            logger.LogStartup();

            IServiceCollection services = builder.Services;
            services.AddControllersWithViews();
            services.ConfigureTelegramBotMvc();

            Bot bot = await Bot.TryCreateAsync(config, CancellationToken.None)
                      ?? throw new InvalidOperationException("Failed to initialize bot due to invalid configuration.");
            services.AddSingleton(bot);
            services.AddHostedService<BotService>();

            WebApplication app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseCors();

            UseUpdateEndpoint(app, config.Token);

            await app.RunAsync();
        }
        catch (Exception ex)
        {
            logger.LogException(ex);
        }
    }

    private static Config? Configure(WebApplicationBuilder builder)
    {
        ConfigurationManager configuration = builder.Configuration;
        Config? config = configuration.Get<Config>();
        if (config is null)
        {
            return null;
        }

        builder.Services.AddOptions<Config>().Bind(configuration).ValidateDataAnnotations();
        builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<Config>>().Value);

        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(config.CultureInfoName);

        return config;
    }

    private static void UseUpdateEndpoint(IApplicationBuilder app, string token)
    {
        object defaults = new
        {
            controller = "Update",
            action = "Post"
        };
        app.UseEndpoints(endpoints => endpoints.MapControllerRoute("update", token, defaults));
    }
}