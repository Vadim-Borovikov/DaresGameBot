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
using AbstractBot.Configs.MessageTemplates;
using DaresGameBot.Game.Matchmaking;
using DaresGameBot.Game.Data.Cards;

namespace DaresGameBot;

public sealed class Bot : BotWithSheets<Config, Texts, object, CommandDataSimple>
{
    public Bot(Config config) : base(config)
    {
        Operations.Add(new UpdateCommand(this));
        Operations.Add(new NewCommand(this));
        Operations.Add(new DrawActionCommand(this));
        Operations.Add(new DrawQuestionCommand(this));
        Operations.Add(new UpdatePlayersOperation(this));

        GoogleSheetsManager.Documents.Document document = DocumentsManager.GetOrAdd(Config.GoogleSheetId);

        _actionsSheet = document.GetOrAddSheet(Config.Texts.ActionsTitle);
        _questionsSheet = document.GetOrAddSheet(Config.Texts.QuestionsTitle);
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken);

        Chat chat = new()
        {
            Id = Config.LogsChatId,
            Type = ChatType.Private
        };

        await UpdateDecksAsync(chat);
    }

    internal async Task UpdateDecksAsync(Chat chat)
    {
        await using (await StatusMessage.CreateAsync(this, chat, Config.Texts.ReadingDecks))
        {
            List<CardAction> actions = await _actionsSheet.LoadAsync<CardAction>(Config.ActionsRange);
            List<Card> questions = await _questionsSheet.LoadAsync<Card>(Config.QuestionsRange);

            _manager = new Game.Manager(this, actions, questions);
        }

        Contexts.Remove(chat.Id);
    }

    internal async Task UpdatePlayersAsync(Chat chat, List<Player> players,
        Dictionary<string, GroupBasedCompatibilityPlayerInfo> compatibilityInfos)
    {
        Game.Data.Game? game = TryGetContext<Game.Data.Game>(chat.Id);
        if (game is null)
        {
            Contexts[chat.Id] = await StartNewGameAsync(chat, players, compatibilityInfos);
            return;
        }
        await _manager!.UpdatePlayersAsync(chat, game, players, compatibilityInfos);
    }

    internal Task OnNewGameAsync(Chat chat)
    {
        MessageTemplateText message = Config.Texts.NewGame;
        message.KeyboardProvider = KeyboardProvider.Remove;
        return message.SendAsync(this, chat);
    }

    internal async Task DrawAsync(Chat chat, int replyToMessageId, bool action = true)
    {
        Game.Data.Game? game = TryGetContext<Game.Data.Game>(chat.Id);
        if (game is null)
        {
            await OnNewGameAsync(chat);
            return;
        }

        Turn turn = action ? game.DrawAction() : game.DrawQuestion();
        if (game.IsActive)
        {
            await _manager!.RepotTurnAsync(chat, game, turn, replyToMessageId);
        }
        else
        {
            Contexts.Remove(chat.Id);
            await Config.Texts.GameOver.SendAsync(this, chat);
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

    private async Task<Game.Data.Game> StartNewGameAsync(Chat chat, List<Player> players,
        Dictionary<string, GroupBasedCompatibilityPlayerInfo> compatibilityInfos)
    {
        Compatibility compatibility = new GroupBasedCompatibility(compatibilityInfos);
        Game.Data.Game game = _manager!.StartNewGame(players, compatibility);
        Contexts[chat.Id] = game;
        await _manager.RepotNewGameAsync(chat, game);
        return game;
    }

    private static ReplyKeyboardMarkup GetKeyboard(params string[] buttonCaptions)
    {
        return new ReplyKeyboardMarkup(buttonCaptions.Select(c => new KeyboardButton(c)));
    }

    private Game.Manager? _manager;
    private readonly Sheet _actionsSheet;
    private readonly Sheet _questionsSheet;
}