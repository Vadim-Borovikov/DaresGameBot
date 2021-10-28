using System.Globalization;
using System.Threading.Tasks;
using AbstractBot;
using DaresGameBot.Bot.Commands;
using DaresGameBot.Game;
using Telegram.Bot.Types;

namespace DaresGameBot.Bot
{
    public sealed class Bot : BotBaseGoogleSheets<Bot, BotConfig>
    {
        public Bot(BotConfig config) : base(config)
        {
            Commands.Add(new StartCommand(this));
            Commands.Add(new NewCommand(this));
            Commands.Add(new DrawActionCommand(this));
            Commands.Add(new DrawQuestionCommand(this));
        }

        protected override async Task UpdateAsync(Message message, CommandBase<Bot, BotConfig> command,
            bool fromChat = false)
        {
            if (command != null)
            {
                await command.ExecuteAsync(message, fromChat);
                return;
            }

            if (ushort.TryParse(message.Text, out ushort playersAmount))
            {
                bool success = await Manager.ChangePlayersAmountAsync(playersAmount, this, message.Chat);
                if (success)
                {
                    return;
                }
            }

            if (float.TryParse(message.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out float choiceChance))
            {
                bool success = await Manager.ChangeChoiceChanceAsync(choiceChance, this, message.Chat);
                if (success)
                {
                    return;
                }
            }

            await Client.SendStickerAsync(message.Chat, DontUnderstandSticker);
        }
    }
}