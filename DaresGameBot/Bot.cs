using DaresGameBot.Operations;
using GoogleSheetsManager.Documents;
using System.Collections.Generic;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using GryphonUtilities.Extensions;
using System.Linq;
using AbstractBot;
using AbstractBot.Bots;
using AbstractBot.Operations.Data;
using DaresGameBot.Operations.Commands;
using DaresGameBot.Configs;

namespace DaresGameBot;

public sealed class Bot : BotWithSheets<Config, Texts, object, CommandDataSimple>
{
    internal readonly Game.Manager GameManager;

    internal readonly Sheet Actions;
    internal readonly Sheet Questions;

    public Bot(Config config) : base(config)
    {
        GameManager = new Game.Manager(this);

        GoogleSheetsManager.Documents.Document document = DocumentsManager.GetOrAdd(config.GoogleSheetId);

        Actions = document.GetOrAddSheet(config.Texts.ActionsTitle);
        Questions = document.GetOrAddSheet(config.Texts.QuestionsTitle);

        Operations.Add(new NewCommand(this));
        Operations.Add(new DrawActionCommand(this));
        Operations.Add(new DrawQuestionCommand(this));
        Operations.Add(new UpdatePlayersAmountOperation(this));
        Operations.Add(new UpdateChoiceChanceOperation(this));
    }

    protected override KeyboardProvider GetDefaultKeyboardProvider(Chat chat)
    {
        return GameManager.CheckGame(chat) ? GameKeyboard : NewGameKeyboard;
    }

    private static ReplyKeyboardMarkup GetKeyboard(IEnumerable<string> buttonCaptions)
    {
        return new ReplyKeyboardMarkup(buttonCaptions.Select(c => new KeyboardButton(c)));
    }

    private static readonly ReplyKeyboardMarkup GameKeyboard = GetKeyboard(Game.Game.GameCaptions);
    private static readonly ReplyKeyboardMarkup NewGameKeyboard = GetKeyboard(Game.Game.NewGameCaption.Yield());
}