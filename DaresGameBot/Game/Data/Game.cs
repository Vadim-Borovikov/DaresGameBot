using System.Collections.Generic;
using System.Linq;
using AbstractBot;
using DaresGameBot.Configs;
using DaresGameBot.Game.ActionCheck;
using DaresGameBot.Game.Data.Cards;
using DaresGameBot.Game.Data.Decks;
using DaresGameBot.Game.Data.Players;

namespace DaresGameBot.Game.Data;

internal sealed class Game : Context
{
    public enum ActionDecksStatus
    {
        BeforeDeck,
        InDeck,
        AfterAllDecks
    }

    public readonly CompanionsSelector CompanionsSelector;

    public ActionDecksStatus Status { get; private set; }

    public IEnumerable<string> PlayerNames => _players.EnumerateNames();

    public Game(Config config, IEnumerable<Player> players, Queue<ActionDeck> actionDecks, QuestionDeck questionsDeck,
        CompanionsSelector companionsSelector)
    {
        Status = ActionDecksStatus.BeforeDeck;

        _config = config;
        _actionDecks = actionDecks;
        _questionsDeck = questionsDeck;
        CompanionsSelector = companionsSelector;

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

        CardAction? action = deck.TrySelectCardFor(_players.Current);
        CompanionsInfo? companions = null;
        if (action is not null)
        {
            companions = CompanionsSelector.TrySelectCompanionsFor(_players.Current, action);
        }
        if (action is null || companions is null)
        {
            _actionDecks.Dequeue();
            _shouldUpdatePossibilities = _actionDecks.Any();
            Status = _actionDecks.Any() ? ActionDecksStatus.BeforeDeck : ActionDecksStatus.AfterAllDecks;
            return null;
        }

        Status = ActionDecksStatus.InDeck;
        return new Turn(_config.Texts, _config.ImagesFolder, action.Tag, action.Description, companions,
            action.ImagePath);
    }

    public Turn DrawQuestion()
    {
        Card question = _questionsDeck.Draw();
        return new Turn(_config.Texts, _config.ImagesFolder, _config.Texts.QuestionsTag, question.Description);
    }

    public void UpdatePlayers(IEnumerable<Player> players)
    {
        _players = new PlayerRepository(players);
        _shouldUpdatePossibilities = true;
    }

    private readonly Config _config;
    private readonly Queue<ActionDeck> _actionDecks;
    private readonly QuestionDeck _questionsDeck;
    private PlayerRepository _players = null!;
    private bool _shouldUpdatePossibilities;
}