using Newtonsoft.Json;

namespace CardsExporter
{
    internal sealed class Configuration
    {
        [JsonProperty]
        public string GoogleProjectJsonPath { get; set; }

        [JsonProperty]
        public string SheetId { get; set; }

        [JsonProperty]
        public string ResultPath { get; set; }
    }
}