namespace DaresGame.Bot.Web.Models
{
    public class BotConfiguration
    {
        public string Token { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public ushort InitialPlayersNumber { get; set; }

        public float ChoiceChance { get; set; }

        public string DecksFolderPath { get; set; }

        public string Url => $"{Host}:{Port}/{Token}";
    }
}