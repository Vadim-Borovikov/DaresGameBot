using System;
using System.Collections.Generic;
using DaresGameBot.Logic;
using GoogleSheetsManager;

namespace DaresGameBot.Web.Models
{
    internal sealed class LoadableCard : Card, ILoadable
    {
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