using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using AbstractBot;
using AbstractBot.Commands;
using AbstractBot.GoogleSheets;
using DaresGameBot.Commands;
using DaresGameBot.Game;
using Telegram.Bot.Types;

namespace DaresGameBot;

public sealed class Bot : BotBaseCustom<Config>, IBotGoogleSheets
{
    public GoogleSheetsComponent GoogleSheetsComponent { get; init; }

    public Bot(Config config) : base(config)
    {
        GoogleSheetsComponent =
            new GoogleSheetsComponent(config, JsonSerializerOptionsProvider.PascalCaseOptions, TimeManager);
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        Commands.Add(new NewCommand(this));
        Commands.Add(new DrawActionCommand(this));
        Commands.Add(new DrawQuestionCommand(this));

        return base.StartAsync(cancellationToken);
    }

    public void Dispose() => GoogleSheetsComponent.Dispose();

    protected override async Task ProcessTextMessageAsync(Message textMessage, Chat senderChat,
        CommandBase? command = null, string? payload = null)
    {
        if (command is not null)
        {
            await command.ExecuteAsync(textMessage, payload);
            return;
        }

        if (ushort.TryParse(textMessage.Text, out ushort playersAmount))
        {
            bool success = await Manager.ChangePlayersAmountAsync(playersAmount, this, textMessage.Chat);
            if (success)
            {
                return;
            }
        }

        if (float.TryParse(textMessage.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out float choiceChance))
        {
            bool success = await Manager.ChangeChoiceChanceAsync(choiceChance, this, textMessage.Chat);
            if (success)
            {
                return;
            }
        }

        await SendStickerAsync(textMessage.Chat, DontUnderstandSticker);
    }
}