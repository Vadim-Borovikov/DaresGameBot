namespace DaresGameBot.Web;

internal static class Program
{
    public static void Main(string[] args)
    {
        try
        {
            CreateHostBuilder(args).Build().Run();
        }
        catch (Exception ex)
        {
            Utils.LogException(ex);
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