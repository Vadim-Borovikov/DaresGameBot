// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
using System;
using System.Collections.Generic;
using DaresGame.Logic;
using Newtonsoft.Json;

namespace DaresGame.Bot.Web.Models
{
    internal class BotConfiguration
    {
        public string Token { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public int PingPeriodSeconds { get; set; }

        public TimeSpan PingPeriod => TimeSpan.FromSeconds(PingPeriodSeconds);

        public ushort InitialPlayersAmount { get; set; }

        public float InitialChoiceChance { get; set; }

        public string DecksJson { get; set; }

        public List<Deck> Decks => JsonConvert.DeserializeObject<List<Deck>>(DecksJson);

        public string Url => $"{Host}:{Port}/{Token}";

        public List<string> ManualLines { get; set; }

        public List<string> AdditionalCommandsLines { get; set; }
    }
}