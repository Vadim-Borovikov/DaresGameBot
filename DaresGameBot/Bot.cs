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

        Turn.Format = Config.Texts.TurnFormat;
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

    internal async Task UpdatePlayersAsync(Chat chat, List<Player> players)
    {
        Game.Data.Game? game = TryGetContext<Game.Data.Game>(chat.Id);
        if (game is null)
        {
            Contexts[chat.Id] = await StartNewGameAsync(chat, players);
            return;
        }
        await _manager!.UpdatePlayersAsync(chat, game, players);
    }

    internal async Task DrawAsync(Chat chat, int replyToMessageId, bool action = true)
    {
        Game.Data.Game? game = TryGetContext<Game.Data.Game>(chat.Id);
        if (game is null)
        {
            return;
        }

        Turn? turn = _manager!.Draw(game, action);
        if (turn is null)
        {
            Contexts.Remove(chat.Id);
            await Config.Texts.GameOver.SendAsync(this, chat);
        }
        else
        {
            await _manager!.RepotTurnAsync(chat, game, turn, replyToMessageId);
        }
    }

    protected override KeyboardProvider GetDefaultKeyboardProvider(Chat chat)
    {
        Game.Data.Game? game = TryGetContext<Game.Data.Game>(chat.Id);

        if (game is null || !game.IsActive)
        {
            return GetKeyboard(Config.Texts.NewGameCaption);
        }

        if (game.Fresh)
        {
            return GetKeyboard(Config.Texts.DrawActionCaption);
        }

        return GetKeyboard(Config.Texts.DrawActionCaption, Config.Texts.DrawQuestionCaption);
    }

    private async Task<Game.Data.Game> StartNewGameAsync(Chat chat, List<Player> players)
    {
        Game.Data.Game game = _manager!.StartNewGame(players);
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