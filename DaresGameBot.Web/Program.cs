using System.Globalization;
using DaresGameBot.Web.Models;
using GryphonUtilities;
using GryphonUtilities.Time;
using Microsoft.Extensions.Options;

namespace DaresGameBot.Web;

internal static class Program
{
    public static void Main(string[] args)
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
            services.AddSingleton<Cpu.Timer>();
            services.AddControllersWithViews();
            services.ConfigureTelegramBotMvc();

            AddBotTo(services);

            WebApplication app = builder.Build();

            Cpu.Timer cpuTimer = app.Services.GetRequiredService<Cpu.Timer>();
            cpuTimer.Start();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseCors();

            UseUpdateEndpoint(app, config.Token);

            app.Run();
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

    private static void AddBotTo(IServiceCollection services)
    {
        services.AddSingleton<BotSingleton>();
        services.AddHostedService<BotService>();
    }

    private static void UseUpdateEndpoint(IApplicationBuilder app, string token)
    {
        object defaults = new
        {
            controller = "Update",
            action = "Post"
        };
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute("update", token, defaults);
            endpoints.MapControllers();
        });
    }
}