using DaresGameBot.Configs;
using DaresGameBot.Game.Data.Cards;
using DaresGameBot.Game.Data.Decks;
using DaresGameBot.Game.Matchmaking.ActionCheck;
using GryphonUtilities.Helpers;
using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Game.Matchmaking.Interactions;

namespace DaresGameBot.Game.Data;

internal sealed class Game
{
    public enum ActionDecksStatus
    {
        BeforeDeck,
        InDeck,
        AfterAllDecks
    }

    public readonly CompanionsSelector CompanionsSelector;

    public ActionDecksStatus Status { get; private set; }
    public bool IncludeEn { get; private set; }

    public IEnumerable<string> PlayerNames => _players;

    public Game(Config config, IEnumerable<string> players, DecksProvider decksProvider,
        CompanionsSelector companionsSelector, IInteractionSubscriber interactionSubscriber)
    {
        Status = ActionDecksStatus.BeforeDeck;

        _config = config;
        _actionDecks = decksProvider.GetActionDecks(companionsSelector);
        _questionsDeck = decksProvider.GetQuestionDeck();
        CompanionsSelector = companionsSelector;
        _interactionSubscriber = interactionSubscriber;

        UpdatePlayers(players);
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

    public void UpdatePlayers(IEnumerable<string> players)
    {
        _players = new PlayerRepository(players);
        _shouldUpdatePossibilities = true;
    }

    public void ToggleLanguages() => IncludeEn = !IncludeEn;

    private readonly Config _config;
    private readonly Queue<ActionDeck> _actionDecks;
    private readonly QuestionDeck _questionsDeck;
    private PlayerRepository _players = null!;
    private bool _shouldUpdatePossibilities;
    private readonly IInteractionSubscriber _interactionSubscriber;
}