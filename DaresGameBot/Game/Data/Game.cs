using DaresGameBot.Configs;
using DaresGameBot.Game.Data.Cards;
using DaresGameBot.Game.Data.Decks;
using DaresGameBot.Game.Matchmaking.ActionCheck;
using DaresGameBot.Game.Matchmaking.Interactions;
using GryphonUtilities.Helpers;
using System.Collections.Generic;
using System.Linq;
using AbstractBot.Extensions;
using DaresGameBot.Game.Matchmaking;
using DaresGameBot.Operations.Info;

namespace DaresGameBot.Game.Data;

internal sealed class Game
{
    public enum ActionDecksStatus
    {
        BeforeDeck,
        InDeck,
        AfterAllDecks
    }

    public ActionDecksStatus Status { get; private set; }
    public bool IncludeEn { get; private set; }

    public IReadOnlyList<string> Players => _players.AsReadOnly();
    public IEnumerable<string> PlayerLines => _playerLines.AsReadOnly();

    public Game(Config config, DecksProvider decksProvider, Matchmaker matchmaker,
        IInteractionSubscriber interactionSubscriber, PlayersInfo info)
        : this(config, decksProvider, matchmaker, interactionSubscriber)
    {
        UpdatePlayers(info);
    }

    private Game(Config config, DecksProvider decksProvider, Matchmaker matchmaker,
        IInteractionSubscriber interactionSubscriber)
    {
        Status = ActionDecksStatus.BeforeDeck;

        _config = config;
        _players = new PlayerRepository();

        _companionsSelector = new CompanionsSelector(matchmaker, Players);
        _actionDecks = decksProvider.GetActionDecks(_companionsSelector);
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
            CompanionsInfo? companions = _companionsSelector.TrySelectCompanionsFor(_players.Current, action);
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

    public void UpdatePlayers(PlayersInfo info)
    {
        _playerLines = info.Lines;

        _players.ResetWith(info.Players);

        _companionsSelector.Matchmaker.Compatibility.PlayerInfos.Clear();
        _companionsSelector.Matchmaker.Compatibility.PlayerInfos.AddAll(info.InteractabilityInfos);

        OnPlayersChanged();
    }

    public void ToggleLanguages() => IncludeEn = !IncludeEn;

    private void OnPlayersChanged() => _shouldUpdatePossibilities = true;

    private readonly Config _config;
    private readonly Queue<ActionDeck> _actionDecks;
    private readonly QuestionDeck _questionsDeck;
    private readonly PlayerRepository _players;
    private readonly IInteractionSubscriber _interactionSubscriber;
    private bool _shouldUpdatePossibilities;
    private readonly CompanionsSelector _companionsSelector;
    private List<string> _playerLines = new();
}