using System.Globalization;
using System.Threading.Tasks;
using AbstractBot;
using DaresGameBot.Bot.Commands;
using DaresGameBot.Game;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DaresGameBot.Bot;

public sealed class Bot : BotBaseGoogleSheets<Bot, BotConfig>
{
    public Bot(BotConfig config) : base(config)
    {
        Commands.Add(new StartCommand(this));
        Commands.Add(new NewCommand(this));
        Commands.Add(new DrawActionCommand(this));
        Commands.Add(new DrawQuestionCommand(this));
    }

    protected override async Task ProcessTextMessageAsync(Message textMessage, bool fromChat,
        CommandBase<Bot, BotConfig>? command = null, string? payload = null)
    {
        if (command is not null)
        {
            await command.ExecuteAsync(textMessage, fromChat, payload);
            return;
        }

        if (ushort.TryParse(textMessage.Text, out ushort playersAmount))
        {
            bool success = await Manager.ChangePlayersAmountAsync(playersAmount, this, textMessage.Chat.Id);
            if (success)
            {
                return;
            }
        }

        if (float.TryParse(textMessage.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out float choiceChance))
        {
            bool success = await Manager.ChangeChoiceChanceAsync(choiceChance, this, textMessage.Chat.Id);
            if (success)
            {
                return;
            }
        }

        await Client.SendStickerAsync(textMessage.Chat.Id, DontUnderstandSticker);
    }
}
