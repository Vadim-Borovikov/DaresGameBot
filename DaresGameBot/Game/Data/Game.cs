using DaresGameBot.Configs;
using DaresGameBot.Game.Data.Cards;
using DaresGameBot.Game.Data.Decks;
using DaresGameBot.Game.Matchmaking.ActionCheck;
using DaresGameBot.Game.Matchmaking.Interactions;
using GryphonUtilities.Helpers;
using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Game.Matchmaking;
using DaresGameBot.Game.Data.PlayerListUpdates;

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

    public IReadOnlyList<string> Players => _players.Names;

    public Game(Config config, DecksProvider decksProvider, PlayerRepository players, Matchmaker matchmaker,
        List<IInteractionSubscriber> interactionSubscribers)
    {
        Status = ActionDecksStatus.BeforeDeck;

        _config = config;
        _players = players;

        _companionsSelector = new CompanionsSelector(matchmaker, Players);
        _actionDecks = decksProvider.GetActionDecks(_companionsSelector);
        _questionsDeck = decksProvider.GetQuestionDeck();
        _interactionSubscribers = interactionSubscribers;

        OnPlayersChanged();
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
            deck.UpdatePossibilities(Players);
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
                foreach (IInteractionSubscriber subscriber in _interactionSubscribers)
                {
                    subscriber.OnInteraction(companions.Player, companions.Partners, action.CompatablePartners,
                        action.Tag);
                }
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

    public void UpdatePlayers(List<PlayerListUpdate> updates)
    {
        _players.Update(updates);

        OnPlayersChanged();
    }

    public void ToggleLanguages() => IncludeEn = !IncludeEn;

    private void OnPlayersChanged() => _shouldUpdatePossibilities = true;

    private readonly Config _config;
    private readonly Queue<ActionDeck> _actionDecks;
    private readonly QuestionDeck _questionsDeck;
    private readonly PlayerRepository _players;
    private readonly List<IInteractionSubscriber> _interactionSubscribers;
    private bool _shouldUpdatePossibilities;
    private readonly CompanionsSelector _companionsSelector;
}