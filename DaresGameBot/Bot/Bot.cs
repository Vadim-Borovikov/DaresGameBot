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
            Commands.Add(new StartCommand(this));
            Commands.Add(new NewCommand(this));
            Commands.Add(new DrawCommand(this));
        }

        protected override async Task UpdateAsync(Message message, CommandBase<BotConfig> command,
            bool fromChat = false)
        {
            if (command != null)
            {
                await command.ExecuteAsync(message, fromChat);
                return;
            }

            if (ushort.TryParse(message.Text, out ushort playersAmount))
            {
                bool success = await Repository.ChangePlayersAmountAsync(playersAmount, Config,
                    GoogleSheetsProvider, Client, message.Chat);
                if (success)
                {
                    return;
                }
            }

            if (float.TryParse(message.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out float choiceChance))
            {
                bool success = await Repository.ChangeChoiceChanceAsync(choiceChance, Config,
                    GoogleSheetsProvider, Client, message.Chat);
                if (success)
                {
                    return;
                }
            }

            await Client.SendStickerAsync(message, DontUnderstandSticker);
        }
    }
}