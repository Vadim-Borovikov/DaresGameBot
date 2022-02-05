using DaresGameBot.Web.Models;

namespace DaresGameBot.Web;

public sealed class Startup
{
    public Startup(IConfiguration config) => _config = config;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<BotSingleton>();
        services.AddHostedService<BotService>();
        services.Configure<Config>(_config);

        services.AddControllersWithViews().AddNewtonsoftJson();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();
        app.UseCors();

        Config botConfig = _config.Get<Config>();
        string token = botConfig.Token ?? throw new NullReferenceException(nameof(botConfig.Token));
        object defaults = new
        {
            controller = "Update",
            action = "Post"
        };
        app.UseEndpoints(endpoints => endpoints.MapControllerRoute("update", token, defaults));
    }

    private readonly IConfiguration _config;
}
