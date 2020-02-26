// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
namespace DaresGame.Bot.Web.Models
{
    internal class BotConfiguration
    {
        public string Token { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public ushort InitialPlayersAmount { get; set; }

        public float ChoiceChance { get; set; }

        public string DecksFolderPath { get; set; }

        public string Url => $"{Host}:{Port}/{Token}";
    }
}