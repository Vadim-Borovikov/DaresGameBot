using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using AbstractBot;
using AbstractBot.Commands;
using GoogleSheetsManager.Providers;
using DaresGameBot.Commands;
using DaresGameBot.Game;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DaresGameBot;

public sealed class Bot : BotBaseCustom<Config>, IDisposable
{
    internal readonly SheetsProvider GoogleSheetsProvider;

    public Bot(Config config) : base(config)
    {
        GoogleSheetsProvider = new SheetsProvider(config, config.GoogleSheetId);
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        Commands.Add(new NewCommand(this));
        Commands.Add(new DrawActionCommand(this));
        Commands.Add(new DrawQuestionCommand(this));

        return base.StartAsync(cancellationToken);
    }

    public void Dispose() => GoogleSheetsProvider.Dispose();

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

    protected override IReplyMarkup GetDefaultKeyboard(Chat chat)
    {
        return Manager.CheckGame(this, chat) ? Utils.GameKeyboard : Utils.NewGameKeyboard;
    }
}