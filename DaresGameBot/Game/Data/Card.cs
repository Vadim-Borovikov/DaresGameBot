using System.Collections.Generic;
using GoogleSheetsManager;

namespace DaresGameBot.Game.Data
{
    internal class Card : ILoadable
    {
        public string Description { get; private set; }

        public virtual void Load(IDictionary<string, object> valueSet)
        {
            Description = valueSet[DescriptionTitle]?.ToString();
        }

        private const string DescriptionTitle = "Текст";
    }
}