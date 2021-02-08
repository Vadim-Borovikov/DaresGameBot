using System;
using System.Collections.Generic;
using GoogleSheetsManager;

namespace DaresGameBot.Game.Data
{
    internal sealed class Card : ILoadable
    {
        public string Description { get; private set; }
        public ushort Players { get; private set; }
        public ushort PartnersToAssign { get; private set; }

        public string Tag { get; private set; }

        public void Load(IList<object> values)
        {
            Players = values.ToUshort(0) ?? throw new ArgumentNullException("Empty players");
            PartnersToAssign = values.ToUshort(1) ?? throw new ArgumentNullException("Empty players to assign");
            Description = values.ToString(2);
            Tag = values.ToString(3);
        }
    }
}