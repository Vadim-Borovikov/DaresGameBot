using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DaresGame.Logic;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace CardsExporter
{
    internal static class Program
    {
        private static void Main()
        {
            Configuration config = GetConfig();

            string csv = GetCsv(config.GoogleProjectJsonPath, config.SheetId);
            IEnumerable<Deck> decks = GetDecks(csv);
            string json = JsonConvert.SerializeObject(decks);
            string propertyValue = json.Replace("\"", "\\\"");

            File.WriteAllText(config.ResultPath, propertyValue);
            ShowFile(config.ResultPath);
        }

        private static Configuration GetConfig()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build()
                .Get<Configuration>();
        }

        private static DriveService CreateDriveService(string googleProjectJsonPath)
        {
            string projectJson = File.ReadAllText(googleProjectJsonPath);
            GoogleCredential credential = GoogleCredential.FromJson(projectJson).CreateScoped(Scopes);

            var initializer = new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            };

            return new DriveService(initializer);
        }

        private static string GetCsv(string googleProjectJsonPath, string sheetId)
        {
            using (DriveService driveService = CreateDriveService(googleProjectJsonPath))
            {
                using (var stream = new MemoryStream())
                {
                    driveService.Files.Export(sheetId, CsvMimeType).Download(stream);
                    stream.Position = 0;
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        private static IEnumerable<Deck> GetDecks(IEnumerable<string> lines)
        {
            var decks = new Dictionary<string, Deck>();
            foreach (string line in lines)
            {
                string[] parts = line.Split(',');
                string tag = parts.Last();
                Deck deck;
                if (decks.ContainsKey(tag))
                {
                    deck = decks[tag];
                }
                else
                {
                    deck = new Deck
                    {
                        Tag = tag,
                        Cards = new List<Card>()
                    };
                    decks.Add(tag, deck);
                }

                var card = new Card
                {
                    Players = int.Parse(parts[0]),
                    PartnersToAssign = int.Parse(parts[1]),
                    Description = string.Join(',', parts.Skip(2).SkipLast(2)).Replace("\"", "")
                };

                deck.Cards.Add(card);
            }
            return decks.Values;
        }

        private static IEnumerable<Deck> GetDecks(string csv)
        {
            IEnumerable<string> lines = csv.Split("\r\n").Skip(1);
            return GetDecks(lines);
        }

        private static void ShowFile(string path)
        {
            string argument = $"/select, {path}";
            Process.Start("explorer.exe", argument);
        }

        private static readonly string[] Scopes = { DriveService.Scope.Drive };
        private const string ApplicationName = "GoogleApisDriveProvider";
        private const string CsvMimeType = "text/csv";
    }
}
