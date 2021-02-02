using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DaresGameBot.Web.Models.Commands;
using GoogleSheetsManager;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace DaresGameBot.Web.Models
{
    public sealed class Bot : IDisposable
    {
        public Bot(IOptions<Config.Config> options)
        {
            _config = options.Value;

            _client = new TelegramBotClient(_config.Token);

            string googleCredentialsJson = _config.GoogleCredentialsJson;
            if (string.IsNullOrWhiteSpace(googleCredentialsJson))
            {
                googleCredentialsJson = JsonConvert.SerializeObject(_config.GoogleCredentials);
            }
            _googleSheetsProvider = new Provider(googleCredentialsJson, ApplicationName, _config.GoogleSheetId);

            _commands = new List<Command>();
            _commands.Add(new StartCommand(_commands, _config.ManualLines, _config.AdditionalCommandsLines, _config,
                _googleSheetsProvider));
            _commands.Add(new NewCommand(_config, _googleSheetsProvider));
            _commands.Add(new DrawCommand(_config, _googleSheetsProvider));

            _dontUnderstandSticker = new InputOnlineFile(_config.DontUnderstandStickerFileId);
        }

        internal Task StartAsync(CancellationToken cancellationToken)
        {
            return _client.SetWebhookAsync(_config.Url, cancellationToken: cancellationToken);
        }

        internal async Task UpdateAsync(Update update)
        {
            if (update?.Type != UpdateType.Message)
            {
                return;
            }

            Message message = update.Message;
            bool fromChat = message.Chat.Id != message.From.Id;
            string botName = fromChat ? await _client.GetNameAsync() : null;

            int replyToMessageId = fromChat ? message.MessageId : 0;

            Command command = _commands.FirstOrDefault(c => c.IsInvokingBy(message, fromChat, botName));
            if (command != null)
            {
                await command.ExecuteAsync(message.Chat.Id, replyToMessageId, _client);
                return;
            }

            if (ushort.TryParse(message.Text, out ushort playersAmount))
            {
                bool success = await GamesRepository.ChangePlayersAmountAsync(playersAmount, _config,
                    _googleSheetsProvider, _client, message.Chat, replyToMessageId);
                if (success)
                {
                    return;
                }
            }

            if (float.TryParse(message.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out float choiceChance))
            {
                bool success = await GamesRepository.ChangeChoiceChanceAsync(choiceChance, _config,
                    _googleSheetsProvider, _client, message.Chat, replyToMessageId);
                if (success)
                {
                    return;
                }
            }

            await _client.SendStickerAsync(message, _dontUnderstandSticker);
        }

        internal Task StopAsync(CancellationToken cancellationToken) => _client.DeleteWebhookAsync(cancellationToken);

        public void Dispose() => _googleSheetsProvider?.Dispose();

        internal Task<User> GetUserAsunc() => _client.GetMeAsync();

        private readonly TelegramBotClient _client;
        private readonly Config.Config _config;
        private readonly List<Command> _commands;
        private readonly InputOnlineFile _dontUnderstandSticker;
        private readonly Provider _googleSheetsProvider;

        private const string ApplicationName = "DaresGameBot";
    }
}