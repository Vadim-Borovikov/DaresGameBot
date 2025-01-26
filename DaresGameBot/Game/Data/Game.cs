﻿using DaresGameBot.Configs;
using DaresGameBot.Game.Data.Cards;
using DaresGameBot.Game.Data.Decks;
using DaresGameBot.Game.Matchmaking.ActionCheck;
using DaresGameBot.Game.Matchmaking.Interactions;
using System.Collections.Generic;
using DaresGameBot.Game.Matchmaking;
using DaresGameBot.Game.Data.PlayerListUpdates;

namespace DaresGameBot.Game.Data;

internal sealed class Game
{
    public Action GetAction(ushort id) => _actionDeck.Cards[id];

    public bool IncludeEn { get; private set; }

    public IReadOnlyList<string> Players => _players.Names;

    public Game(Config config, DecksProvider decksProvider, PlayerRepository players, Matchmaker matchmaker,
        List<IInteractionSubscriber> interactionSubscribers)
    {
        _config = config;
        _players = players;

        _companionsSelector = new CompanionsSelector(matchmaker, Players);
        _actionDeck = decksProvider.GetActionDeck(_companionsSelector);
        _questionsDeck = decksProvider.GetQuestionDeck();
        _interactionSubscribers = interactionSubscribers;

        _players.UpdateActions(_actionDeck);
    }

    public ActionInfo DrawAction()
    {
        ushort actionId = _actionDeck.SelectCard(_players);
        return _companionsSelector.SelectCompanionsFor(_players.Current, actionId, _actionDeck.Cards[actionId]);
    }

    public void RegisterAction(ActionInfo info, ushort points)
    {
        Action action = GetAction(info.ActionId);

        foreach (IInteractionSubscriber subscriber in _interactionSubscribers)
        {
            subscriber.OnInteraction(info.Player, info.Partners, action.CompatablePartners, points);
        }

        _players.MoveNext();
    }

    public void RegisterQuestion() => _players.MoveNext();

    public Turn DrawQuestion(string player)
    {
        Question question = _questionsDeck.Draw();
        return new Turn(_config.Texts, _config.ImagesFolder, _config.Texts.QuestionsTag, question.Description,
            question.DescriptionEn, player);
    }

    public ushort UpdatePlayers(List<PlayerListUpdate> updates)
    {
        ushort pointsForNewPlayers = _players.UpdateList(updates);
        _players.UpdateActions(_actionDeck);
        return pointsForNewPlayers;
    }

    public void ToggleLanguages() => IncludeEn = !IncludeEn;

    private readonly Config _config;
    private readonly ActionDeck _actionDeck;
    private readonly QuestionDeck _questionsDeck;
    private readonly PlayerRepository _players;
    private readonly List<IInteractionSubscriber> _interactionSubscribers;
    private readonly CompanionsSelector _companionsSelector;
}