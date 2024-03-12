using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AbstractBot.Configs.MessageTemplates;
using DaresGameBot.Game.Data;
using DaresGameBot.Game.Data.Cards;
using DaresGameBot.Game.Data.Decks;
using DaresGameBot.Game.Data.Players;
using DaresGameBot.Game.Matchmaking;
using DaresGameBot.Game.Matchmaking.ActionCheck;
using DaresGameBot.Game.Matchmaking.Interactions;
using DaresGameBot.Game.Matchmaking.PlayerCheck;
using Telegram.Bot.Types;

namespace DaresGameBot.Game;

internal sealed class Manager
{
    public Manager(Bot bot, IReadOnlyList<CardAction> actions, IReadOnlyList<Card> questions)
    {
        for (ushort i = 0; i < actions.Count; i++)
        {
            actions[i].Id = i;
        }

        _bot = bot;
        _actions = actions;
        _questions = questions;
    }

    public Data.Game StartNewGame(List<Player> players, Compatibility compatibility)
    {
        InteractionRepository interactionRepository = new();
        DistributedMatchmaker matchmaker = new(compatibility, interactionRepository);
        CompanionsSelector companionsSelector = new(matchmaker, players);
        Queue<ActionDeck> actionDecks = GetActionDecks(companionsSelector);
        QuestionDeck questionsDeck = new(_questions);
        return
            new Data.Game(_bot.Config, players, actionDecks, questionsDeck, companionsSelector, interactionRepository);
    }

    public Task RepotNewGameAsync(Chat chat, Data.Game game)
    {
        MessageTemplateText playersText =
            _bot.Config.Texts.PlayersFormat.Format(string.Join(PlayerSeparator, game.PlayerNames));
        MessageTemplateText startText = _bot.Config.Texts.NewGameFormat.Format(playersText);
        return startText.SendAsync(_bot, chat);
    }

    public Task UpdatePlayersAsync(Chat chat, Data.Game game, IEnumerable<Player> players,
        Dictionary<string, IPartnerChecker> infos)
    {
        game.UpdatePlayers(players);
        Compatibility compatibility = new(infos);
        game.CompanionsSelector.Matchmaker.Compatibility = compatibility;

        MessageTemplateText playersText =
            _bot.Config.Texts.PlayersFormat.Format(string.Join(PlayerSeparator, game.PlayerNames));
        MessageTemplateText messageText = _bot.Config.Texts.AcceptedFormat.Format(playersText);
        return messageText.SendAsync(_bot, chat);
    }

    public async Task RepotTurnAsync(Chat chat, Data.Game game, Turn turn, int replyToMessageId)
    {
        MessageTemplate message = turn.GetMessage(game.PlayerNames.Count(), game.IncludeEn);
        message.ReplyToMessageId = replyToMessageId;
        await message.SendAsync(_bot, chat);
    }

    private Queue<ActionDeck> GetActionDecks(IActionChecker checker)
    {
        IEnumerable<ActionDeck> decks = _actions.GroupBy(c => c.Tag).Select(g => new ActionDeck(g, checker));
        return new Queue<ActionDeck>(decks);
    }

    private readonly Bot _bot;
    private readonly IReadOnlyList<CardAction> _actions;
    private readonly IReadOnlyList<Card> _questions;

    private const string PlayerSeparator = ", ";
}