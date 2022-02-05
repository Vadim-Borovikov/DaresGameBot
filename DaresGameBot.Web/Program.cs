using AbstractBot;

namespace DaresGameBot.Web;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        Utils.DeleteExceptionLog();
        try
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }
        catch (Exception ex)
        {
            await Utils.LogExceptionAsync(ex);
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
                   .ConfigureLogging((context, builder) =>
                   {
                       builder.AddConfiguration(context.Configuration.GetSection("Logging"));
                       builder.AddFile(o => o.RootPath = context.HostingEnvironment.ContentRootPath);
                   })
                   .ConfigureWebHostDefaults(builder => builder.UseStartup<Startup>());
    }
}