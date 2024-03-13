using DaresGameBot.Configs;
using DaresGameBot.Game.Data.Cards;
using DaresGameBot.Game.Data.Decks;
using DaresGameBot.Game.Matchmaking.ActionCheck;
using DaresGameBot.Game.Matchmaking.Interactions;
using GryphonUtilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using AbstractBot.Extensions;
using DaresGameBot.Game.Matchmaking;
using DaresGameBot.Game.Matchmaking.PlayerCheck;

namespace DaresGameBot.Game.Data;

internal sealed class Game
{
    public enum ActionDecksStatus
    {
        BeforeDeck,
        InDeck,
        AfterAllDecks
    }

    public readonly Guid Id;

    public readonly CompanionsSelector CompanionsSelector;

    public ActionDecksStatus Status { get; private set; }
    public bool CanBeJoined { get; private set; }
    public bool IncludeEn { get; private set; }

    public IReadOnlyList<string> Players => _players.AsReadOnly();

    public Game(Config config, DecksProvider decksProvider, Matchmaker matchmaker,
        IInteractionSubscriber interactionSubscriber, IEnumerable<string> players)
        : this(config, decksProvider, matchmaker, interactionSubscriber)
    {
        UpdatePlayers(players);
    }

    public Game(Config config, DecksProvider decksProvider, Matchmaker matchmaker,
        IInteractionSubscriber interactionSubscriber, string player, IPartnerChecker checker)
        : this(config, decksProvider, matchmaker, interactionSubscriber)
    {
        AddPlayer(player, checker);
        CanBeJoined = true;
    }

    private Game(Config config, DecksProvider decksProvider, Matchmaker matchmaker,
        IInteractionSubscriber interactionSubscriber)
    {
        Id = Guid.NewGuid();

        Status = ActionDecksStatus.BeforeDeck;

        _config = config;
        _players = new PlayerRepository();

        CompanionsSelector = new CompanionsSelector(matchmaker, Players);
        _actionDecks = decksProvider.GetActionDecks(CompanionsSelector);
        _questionsDeck = decksProvider.GetQuestionDeck();
        _interactionSubscriber = interactionSubscriber;
    }

    public Turn? TryDrawAction()
    {
        switch (Status)
        {
            case ActionDecksStatus.AfterAllDecks:
                return null;
            case ActionDecksStatus.InDeck:
                _players.MoveNext();
                break;
        }

        ActionDeck deck = _actionDecks.Peek();
        if (_shouldUpdatePossibilities)
        {
            deck.UpdatePossibilities(_players);
            _shouldUpdatePossibilities = false;
        }
        if (!deck.IsEmpty())
        {
            Status = ActionDecksStatus.InDeck;
            CardAction? action = deck.TrySelectCardFor(_players.Current);
            if (action is null)
            {
                return null;
            }
            CompanionsInfo? companions = CompanionsSelector.TrySelectCompanionsFor(_players.Current, action);
            if (companions is null)
            {
                return null;
            }

            if (companions.Partners is not null)
            {
                _interactionSubscriber.OnInteraction(companions.Player, companions.Partners,
                    action.CompatablePartners);
            }
            return new Turn(_config.Texts, _config.ImagesFolder, action.Tag, null, action.Description,
                action.DescriptionEn, companions, action.ImagePath);
        }

        _actionDecks.Dequeue();
        _shouldUpdatePossibilities = _actionDecks.Any();
        Status = _actionDecks.Any() ? ActionDecksStatus.BeforeDeck : ActionDecksStatus.AfterAllDecks;
        return null;
    }

    public Turn DrawQuestion(bool forPlayerWithNoMatches)
    {
        Card question = _questionsDeck.Draw();
        string? prefix = forPlayerWithNoMatches ? Text.JoinLines(_config.Texts.NoMatchesInDeckLines) : null;
        CompanionsInfo? companions = forPlayerWithNoMatches ? new CompanionsInfo(_players.Current, null, null) : null;
        return new Turn(_config.Texts, _config.ImagesFolder, _config.Texts.QuestionsTag, prefix, question.Description,
            question.DescriptionEn, companions);
    }

    public void AddPlayer(string player, IPartnerChecker checker)
    {
        _players.Add(player);
        CompanionsSelector.Matchmaker.Compatibility.AddPlayer(player, checker);
        OnPlayersChanged();
    }

    public void UpdatePlayers(IEnumerable<string> players, Dictionary<string, IPartnerChecker>? infos = null)
    {
        _players.ResetWith(players);

        if (infos is not null)
        {
            CompanionsSelector.Matchmaker.Compatibility.PlayerInfos.Clear();
            CompanionsSelector.Matchmaker.Compatibility.PlayerInfos.AddAll(infos);
        }

        CanBeJoined = false;
        OnPlayersChanged();
    }

    public void OnPlayersChanged() => _shouldUpdatePossibilities = true;

    public void ToggleLanguages() => IncludeEn = !IncludeEn;

    private readonly Config _config;
    private readonly Queue<ActionDeck> _actionDecks;
    private readonly QuestionDeck _questionsDeck;
    private readonly PlayerRepository _players;
    private readonly IInteractionSubscriber _interactionSubscriber;
    private bool _shouldUpdatePossibilities;
}