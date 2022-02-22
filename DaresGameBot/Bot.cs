using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using AbstractBot;
using DaresGameBot.Commands;
using DaresGameBot.Game;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DaresGameBot;

public sealed class Bot : BotBaseGoogleSheets<Bot, Config>
{
    public Bot(Config config) : base(config) { }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        Commands.Add(new StartCommand(this));
        Commands.Add(new NewCommand(this));
        Commands.Add(new DrawActionCommand(this));
        Commands.Add(new DrawQuestionCommand(this));

        return base.StartAsync(cancellationToken);
    }

    protected override async Task ProcessTextMessageAsync(Message textMessage, bool fromChat,
        CommandBase<Bot, Config>? command = null, string? payload = null)
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
