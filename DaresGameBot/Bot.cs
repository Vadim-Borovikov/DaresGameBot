using DaresGameBot.Operations;
using GoogleSheetsManager.Documents;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AbstractBot;
using AbstractBot.Bots;
using AbstractBot.Operations.Data;
using DaresGameBot.Operations.Commands;
using DaresGameBot.Configs;
using DaresGameBot.Game.Data;
using System.Collections.Generic;
using Telegram.Bot.Types.Enums;

namespace DaresGameBot;

public sealed class Bot : BotWithSheets<Config, Texts, object, CommandDataSimple>
{
    public Bot(Config config) : base(config)
    {
        Operations.Add(new NewCommand(this));
        Operations.Add(new DrawActionCommand(this));
        Operations.Add(new DrawQuestionCommand(this));
        Operations.Add(new UpdatePlayersOperation(this));
        Operations.Add(new UpdateChoiceChanceOperation(this));

        Help.SetArgs(Config.Texts.Choosable);
        Partner.Choosable = Config.Texts.Choosable;

        Turn.Format = Config.Texts.TurnFormat;
        Turn.PartnerFormat = Config.Texts.TurnPartnerFormat;
        Turn.PartnersFormat = Config.Texts.TurnPartnersFormat;
        Turn.Partner = Config.Texts.Partner;
        Turn.Partners = Config.Texts.Partners;
        Turn.PartnersSeparator = Config.Texts.PartnersSeparator;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken);

        Chat chat = new()
        {
            Id = Config.LogsChatId,
            Type = ChatType.Private
        };

        await using (await StatusMessage.CreateAsync(this, chat, Config.Texts.ReadingDecks))
        {
            GoogleSheetsManager.Documents.Document document = DocumentsManager.GetOrAdd(Config.GoogleSheetId);

            Sheet actionsSheet = document.GetOrAddSheet(Config.Texts.ActionsTitle);
            Sheet questionsSheet = document.GetOrAddSheet(Config.Texts.QuestionsTitle);

            List<CardAction> actions = await actionsSheet.LoadAsync<CardAction>(Config.ActionsRange);
            List<Card> questions = await questionsSheet.LoadAsync<Card>(Config.QuestionsRange);

            _manager = new Game.Manager(this, actions, questions);
        }
    }

    internal async Task UpdatePlayersAsync(Chat chat, IEnumerable<string> playerNames)
    {
        Game.Data.Game? game = TryGetContext<Game.Data.Game>(chat.Id);
        if (game is null)
        {
            Contexts[chat.Id] = await StartNewGameAsync(chat, playerNames);
            return;
        }
        await _manager!.UpdatePlayersAsync(chat, game, playerNames);
    }

    internal async Task UpdateChoiceChanceAsync(Chat chat, decimal choiceChance)
    {
        Game.Data.Game? game = TryGetContext<Game.Data.Game>(chat.Id);
        if (game is null)
        {
            return;
        }

        await _manager!.UpdateChoiceChanceAsync(chat, game, choiceChance);
    }

    internal async Task DrawAsync(Chat chat, int replyToMessageId, bool action = true)
    {
        Game.Data.Game? game = TryGetContext<Game.Data.Game>(chat.Id);
        if (game is null)
        {
            return;
        }

        Turn? turn = _manager!.Draw(game, action);
        if (turn is not null)
        {
            await _manager.RepotTurnAsync(chat, game, turn, replyToMessageId);
        }
    }

    protected override KeyboardProvider GetDefaultKeyboardProvider(Chat chat)
    {
        Game.Data.Game? game = TryGetContext<Game.Data.Game>(chat.Id);
        return game is not null && game.IsActive()
            ? GetKeyboard(Config.Texts.DrawActionCaption, Config.Texts.DrawQuestionCaption)
            : GetKeyboard(Config.Texts.NewGameCaption);
    }

    private async Task<Game.Data.Game> StartNewGameAsync(Chat chat, IEnumerable<string> playerNames)
    {
        Game.Data.Game game = _manager!.StartNewGame(playerNames);
        Contexts[chat.Id] = game;
        await _manager.RepotNewGameAsync(chat, game);
        return game;
    }

    private static ReplyKeyboardMarkup GetKeyboard(params string[] buttonCaptions)
    {
        return new ReplyKeyboardMarkup(buttonCaptions.Select(c => new KeyboardButton(c)));
    }

    private Game.Manager? _manager;
}