// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
using System.Collections.Generic;

namespace DaresGame.Bot.Web.Models
{
    internal class BotConfiguration
    {
        public string Token { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public ushort InitialPlayersAmount { get; set; }

        public float InitialChoiceChance { get; set; }

        public string DecksJson { get; set; }

        public string Url => $"{Host}:{Port}/{Token}";

        public List<string> ManualLines { get; set; }

        public List<string> AdditionalCommandsLines { get; set; }
    }
}