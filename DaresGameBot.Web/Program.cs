using System.Globalization;
using DaresGameBot.Configs;
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

            Models.Config config = Configure(builder) ?? throw new NullReferenceException("Can't load config.");
            clock = new Clock(config.SystemTimeZoneIdLogs);
            logger = new Logger(clock);
            logger.LogStartup();

            IServiceCollection services = builder.Services;
            services.AddSingleton<Cpu.Timer>();
            services.AddControllersWithViews();
            services.ConfigureTelegramBotMvc();

            Bot bot = await Bot.TryCreateAsync(config, CancellationToken.None)
                      ?? throw new InvalidOperationException("Failed to initialize bot due to invalid configuration.");
            services.AddSingleton(bot);
            services.AddHostedService<BotService>();

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

            await app.RunAsync();
        }
        catch (Exception ex)
        {
            logger.LogException(ex);
        }
    }

    private static Models.Config? Configure(WebApplicationBuilder builder)
    {
        ConfigurationManager configuration = builder.Configuration;
        Models.Config? config = configuration.Get<Models.Config>();
        if (config is null)
        {
            return null;
        }

        builder.Services.AddOptions<Models.Config>().Bind(configuration).ValidateDataAnnotations();
        builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<Models.Config>>().Value);

        LoadTextsFiles(builder, config);

        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(config.CultureInfoName);

        return config;
    }

    private static void LoadTextsFiles(WebApplicationBuilder builder, Models.Config config)
    {
        foreach (string file in Directory.GetFiles(builder.Environment.ContentRootPath, "texts.*.json"))
        {
            string? langCode = ExtractLanguageCode(file);
            if (langCode is null)
            {
                continue;
            }

            builder.Configuration.AddJsonFile(file, true, true);

            Texts? texts = builder.Configuration.Get<Texts>();
            if (texts is not null)
            {
                config.AllTexts[langCode] = texts;
            }
        }
    }

    private static string? ExtractLanguageCode(string filePath)
    {
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        string[] parts = fileName.Split('.');
        return parts.Length == 2 ? parts[1] : null;
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