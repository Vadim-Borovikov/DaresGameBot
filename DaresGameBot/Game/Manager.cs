using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AbstractBot.Configs.MessageTemplates;
using DaresGameBot.Game.ActionCheck;
using DaresGameBot.Game.Data;
using DaresGameBot.Game.Data.Cards;
using DaresGameBot.Game.Data.Decks;
using DaresGameBot.Game.Data.Players;
using DaresGameBot.Game.Matchmaking;
using Telegram.Bot.Types;

namespace DaresGameBot.Game;

internal sealed class Manager
{
    public Manager(Bot bot, IReadOnlyList<CardAction> actions, IReadOnlyList<Card> questions)
    {
        _bot = bot;
        _actions = actions;
        _questions = questions;
    }

    public Data.Game StartNewGame(List<Player> players, Compatibility compatibility)
    {
        RandomMatchmaker matchmaker = new(compatibility);
        CompanionsSelector companionsSelector = new(matchmaker, players);
        IList<ActionDeck> actionDecks = GetActionDecks(companionsSelector);
        QuestionDeck questionsDeck = new(_questions);
        return new Data.Game(_bot.Config, players, actionDecks, questionsDeck, companionsSelector);
    }

    public Task RepotNewGameAsync(Chat chat, Data.Game game)
    {
        MessageTemplateText playersText =
            _bot.Config.Texts.PlayersFormat.Format(string.Join(PlayerSeparator, game.PlayerNames));
        MessageTemplateText startText = _bot.Config.Texts.NewGameFormat.Format(playersText);
        return startText.SendAsync(_bot, chat);
    }

    public Task UpdatePlayersAsync(Chat chat, Data.Game game, IEnumerable<Player> players,
        Dictionary<string, GroupBasedCompatibilityPlayerInfo> compatibilityInfos)
    {
        game.UpdatePlayers(players);
        GroupBasedCompatibility compatibility = new(compatibilityInfos);
        game.CompanionsSelector.Matchmaker = new RandomMatchmaker(compatibility);

        MessageTemplateText playersText =
            _bot.Config.Texts.PlayersFormat.Format(string.Join(PlayerSeparator, game.PlayerNames));
        MessageTemplateText messageText = _bot.Config.Texts.AcceptedFormat.Format(playersText);
        return messageText.SendAsync(_bot, chat);
    }

    public async Task RepotTurnAsync(Chat chat, Data.Game game, Turn turn, int replyToMessageId)
    {
        MessageTemplate message = turn.GetMessage(game.PlayerNames.Count());
        message.ReplyToMessageId = replyToMessageId;
        await message.SendAsync(_bot, chat);
    }

    private IList<ActionDeck> GetActionDecks(IActionChecker checker)
    {
        return _actions.GroupBy(c => c.Tag).Select(g => new ActionDeck(g, checker)).ToList();
    }

    private readonly Bot _bot;
    private readonly IReadOnlyList<CardAction> _actions;
    private readonly IReadOnlyList<Card> _questions;

    private const string PlayerSeparator = ", ";
}