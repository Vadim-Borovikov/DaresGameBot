using DaresGameBot.Operations;
using GoogleSheetsManager.Documents;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Linq;
using AbstractBot;
using AbstractBot.Bots;
using AbstractBot.Operations.Data;
using DaresGameBot.Operations.Commands;
using DaresGameBot.Configs;
using DaresGameBot.Game;
using DaresGameBot.Game.Data;

namespace DaresGameBot;

public sealed class Bot : BotWithSheets<Config, Texts, object, CommandDataSimple>
{
    internal readonly Repository Repository;

    internal readonly Sheet Actions;
    internal readonly Sheet Questions;

    public Bot(Config config) : base(config)
    {
        Repository = new Repository(this);

        GoogleSheetsManager.Documents.Document document = DocumentsManager.GetOrAdd(config.GoogleSheetId);

        Actions = document.GetOrAddSheet(config.Texts.ActionsTitle);
        Questions = document.GetOrAddSheet(config.Texts.QuestionsTitle);

        Operations.Add(new NewCommand(this));
        Operations.Add(new DrawActionCommand(this));
        Operations.Add(new DrawQuestionCommand(this));
        Operations.Add(new UpdatePlayersAmountOperation(this));
        Operations.Add(new UpdateChoiceChanceOperation(this));

        Help.SetArgs(Config.Texts.Choosable);
        Partner.Choosable = Config.Texts.Choosable;

        Turn.TurnFormat = Config.Texts.TurnFormat;
        Turn.Partner = Config.Texts.Partner;
        Turn.Partners = Config.Texts.Partners;
        Turn.PartnersSeparator = Config.Texts.PartnersSeparator;
    }

    protected override KeyboardProvider GetDefaultKeyboardProvider(Chat chat)
    {
        return Repository.CheckGame(chat)
            ? GetKeyboard(Config.Texts.DrawActionCaption, Config.Texts.DrawQuestionCaption)
            : GetKeyboard(Config.Texts.NewGameCaption);
    }

    private static ReplyKeyboardMarkup GetKeyboard(params string[] buttonCaptions)
    {
        return new ReplyKeyboardMarkup(buttonCaptions.Select(c => new KeyboardButton(c)));
    }
}