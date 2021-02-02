using System.Diagnostics.CodeAnalysis;
using DaresGameBot.Web.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace DaresGameBot.Web
{
    public sealed class Startup
    {
        public Startup(IConfiguration config) => _config = config;

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<BotSingleton>();
            services.AddHostedService<BotService>();
            services.Configure<Config>(_config);

            services.AddMvc();
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            string token = _config.GetSection("BotConfig")["Token"];
            if (string.IsNullOrEmpty(token))
            {
                var botConfig = JsonConvert.DeserializeObject<Bot.Config>(_config["BotConfigJson"]);
                token = botConfig.Token;
            }

            app.UseMvc(routes => routes.MapRoute("update", $"{token}/{{controller=Update}}/{{action=post}}"));
        }

        private readonly IConfiguration _config;
    }
}
