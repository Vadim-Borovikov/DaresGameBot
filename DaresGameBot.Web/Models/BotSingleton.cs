using System;
using DaresGameBot.Bot;
using Microsoft.Extensions.Options;

namespace DaresGameBot.Web.Models
{
    public sealed class BotSingleton : IDisposable
    {
        internal readonly Bot.Bot Bot;

        public BotSingleton(IOptions<Config> options) => Bot = new Bot.Bot(options.Value);

        public void Dispose() => Bot.Dispose();
    }
}