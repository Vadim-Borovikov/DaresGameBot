using System.Collections.Generic;
using System.Linq;
using AbstractBot;
using DaresGameBot.Configs;
using DaresGameBot.Game.ActionCheck;
using DaresGameBot.Game.Data.Cards;
using DaresGameBot.Game.Data.Decks;

namespace DaresGameBot.Game.Data;

internal sealed class Game : Context
{
    public readonly CompanionsSelector CompanionsSelector;
    public IEnumerable<string> PlayerNames => _players.Select(p => p.Name);
    public bool Fresh;

    public bool IsActive => _nextActionTurn is not null;

    public Game(Config config, List<Player> players, IList<ActionDeck> actionDecks, QuestionDeck questionsDeck,
        CompanionsSelector companionsSelector)
    {
        Fresh = true;
        _config = config;
        _actionDecks = actionDecks;
        _questionsDeck = questionsDeck;
        CompanionsSelector = companionsSelector;

        UpdatePlayers(players);

        TryPrepareNextActionTurn();
    }

    public Turn DrawAction()
    {
        if (!Fresh)
        {
            _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
        }

        Fresh = false;
        Turn? result = _nextActionTurn;
        TryPrepareNextActionTurn();
        return result!;
    }

    public Turn DrawQuestion()
    {
        Fresh = false;
        Card question = _questionsDeck.Draw();
        return new Turn(_config.Texts, _config.ImagesFolder, _config.Texts.QuestionsTag, question.Description);
    }

    public void UpdatePlayers(List<Player> players)
    {
        _players = players;
        _currentPlayerIndex = 0;
    }

    private void TryPrepareNextActionTurn()
    {
        int nextPlayerIndex = Fresh ? 0 : (_currentPlayerIndex + 1) % _players.Count;
        Player player = _players[nextPlayerIndex];
        while (_actionDecks.Any())
        {
            ActionDeck deck = _actionDecks.First();
            CardAction? action = deck.TrySelectCardFor(player);
            if (action is not null)
            {
                _nextActionTurn = new Turn(_config.Texts, _config.ImagesFolder, action.Tag, action.Description,
                    CompanionsSelector.CompanionsInfo, action.ImagePath);
                return;
            }
            _actionDecks.RemoveAt(0);
        }

        _nextActionTurn = null;
    }

    private readonly Config _config;
    private readonly IList<ActionDeck> _actionDecks;
    private readonly QuestionDeck _questionsDeck;
    private List<Player> _players = new();
    private int _currentPlayerIndex;
    private Turn? _nextActionTurn;
}
