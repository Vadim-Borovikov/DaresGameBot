using System;
using AbstractBot;
using GoogleSheetsManager.Providers;
using DaresGameBot.Commands;
using DaresGameBot.Game;
using DaresGameBot.Operations;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DaresGameBot;

public sealed class Bot : BotBaseCustom<Config>, IDisposable
{
    internal readonly SheetsProvider GoogleSheetsProvider;

    public Bot(Config config) : base(config)
    {
        GoogleSheetsProvider = new SheetsProvider(config, config.GoogleSheetId);

        Operations.Add(new NewCommand(this));
        Operations.Add(new DrawActionCommand(this));
        Operations.Add(new DrawQuestionCommand(this));
        Operations.Add(new UpdatePlayersAmountOperation(this));
        Operations.Add(new UpdateChoiceChanceOperation(this));
    }

    public void Dispose() => GoogleSheetsProvider.Dispose();

    protected override IReplyMarkup GetDefaultKeyboard(Chat chat)
    {
        return Manager.CheckGame(this, chat) ? Utils.GameKeyboard : Utils.NewGameKeyboard;
    }
}