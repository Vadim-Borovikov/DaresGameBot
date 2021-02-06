using System.Globalization;
using System.Threading.Tasks;
using AbstractBot;
using DaresGameBot.Bot.Commands;
using DaresGameBot.Game;
using Telegram.Bot.Types;

namespace DaresGameBot.Bot
{
    public sealed class Bot : BotBaseGoogleSheets<BotConfig>
    {
        public Bot(BotConfig config) : base(config)
        {
            Commands.Add(new StartCommand(this, Config, GoogleSheetsProvider));
            Commands.Add(new NewCommand(Config, GoogleSheetsProvider));
            Commands.Add(new DrawCommand(Config, GoogleSheetsProvider));
        }

        protected override async Task UpdateAsync(Message message, CommandBase command, bool fromChat = false)
        {
            int replyToMessageId = fromChat ? message.MessageId : 0;
            if (command != null)
            {
                await command.ExecuteAsync(message.Chat.Id, Client, replyToMessageId);
                return;
            }

            if (ushort.TryParse(message.Text, out ushort playersAmount))
            {
                bool success = await Repository.ChangePlayersAmountAsync(playersAmount, Config,
                    GoogleSheetsProvider, Client, message.Chat, replyToMessageId);
                if (success)
                {
                    return;
                }
            }

            if (float.TryParse(message.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out float choiceChance))
            {
                bool success = await Repository.ChangeChoiceChanceAsync(choiceChance, Config,
                    GoogleSheetsProvider, Client, message.Chat, replyToMessageId);
                if (success)
                {
                    return;
                }
            }

            await Client.SendStickerAsync(message, DontUnderstandSticker);
        }
    }
}