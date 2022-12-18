using DaresGameBot.Commands;
using DaresGameBot.Game;
using DaresGameBot.Operations;
using GoogleSheetsManager.Documents;
using System.Collections.Generic;
using System;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using GryphonUtilities.Extensions;
using System.Linq;
using AbstractBot.Bots;

namespace DaresGameBot;

public sealed class Bot : BotWithSheets<Config>
{
    internal readonly Manager GameManager;

    internal readonly Sheet Actions;
    internal readonly Sheet Questions;

    public Bot(Config config) : base(config)
    {
        GameManager = new Manager(this);

        GoogleSheetsManager.Documents.Document document = DocumentsManager.GetOrAdd(config.GoogleSheetId);

        Dictionary<Type, Func<object?, object?>> additionalConverters = new();
        additionalConverters[typeof(ushort)] = additionalConverters[typeof(ushort?)] = o => o.ToUshort();

        Actions = document.GetOrAddSheet(config.ActionsTitle, additionalConverters);
        Questions = document.GetOrAddSheet(config.QuestionsTitle);

        Operations.Add(new NewCommand(this));
        Operations.Add(new DrawActionCommand(this));
        Operations.Add(new DrawQuestionCommand(this));
        Operations.Add(new UpdatePlayersAmountOperation(this));
        Operations.Add(new UpdateChoiceChanceOperation(this));
    }

    protected override IReplyMarkup GetDefaultKeyboard(Chat chat)
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