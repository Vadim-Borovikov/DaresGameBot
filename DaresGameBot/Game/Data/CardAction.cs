using System;
using System.Collections.Generic;
using GoogleSheetsManager;

namespace DaresGameBot.Game.Data
{
    internal sealed class CardAction : Card
    {
        public ushort Players { get; private set; }
        public ushort PartnersToAssign { get; private set; }

        public string Tag { get; private set; }

        public override void Load(IDictionary<string, object> valueSet)
        {
            Players = valueSet[PlayersTitle]?.ToUshort() ?? throw new ArgumentNullException("Empty players");
            PartnersToAssign =
                valueSet[PartnersToAssignTitle]?.ToUshort() ?? throw new ArgumentNullException("Empty players to assign");
            Tag = valueSet[TagTitle]?.ToString();

            base.Load(valueSet);
        }

        private const string PlayersTitle = "Минимум";
        private const string PartnersToAssignTitle = "Назначить";
        private const string TagTitle = "Символ";
    }
}