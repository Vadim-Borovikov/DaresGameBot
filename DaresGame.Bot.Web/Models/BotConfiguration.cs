// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CollectionNeverUpdated.Global
using System.Collections.Generic;
using DaresGame.Logic;

namespace DaresGame.Bot.Web.Models
{
    internal class BotConfiguration
    {
        public string Token { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public ushort InitialPlayersAmount { get; set; }

        public float ChoiceChance { get; set; }

        public List<Deck> Decks { get; set; }

        public string Url => $"{Host}:{Port}/{Token}";
    }
}