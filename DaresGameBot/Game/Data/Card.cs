using System.Collections.Generic;
using GoogleSheetsManager;

namespace DaresGameBot.Game.Data
{
    internal class Card : ILoadable
    {
        public string Description { get; protected set; }

        public virtual void Load(IList<object> values) => Description = values.ToString(0);
    }
}