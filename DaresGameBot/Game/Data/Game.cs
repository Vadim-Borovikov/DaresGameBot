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
    public readonly CompanionsSelector CompanionsSelector;
    public bool Fresh { get; private set; }

    public bool IsActive => _nextActionTurn is not null;
    public IEnumerable<string> PlayerNames => _players.EnumerateNames();

    public Game(Config config, IEnumerable<Player> players, IList<ActionDeck> actionDecks, QuestionDeck questionsDeck,
        CompanionsSelector companionsSelector)
    {
        Fresh = true;
        _config = config;
        _actionDecks = actionDecks;
        _questionsDeck = questionsDeck;
        CompanionsSelector = companionsSelector;

        _players = new PlayerRepository(players);

        _nextActionTurn = TryDrawAction();
    }

    public Turn DrawAction()
    {
        if (!Fresh)
        {
            _players.MoveNext();
        }

        Fresh = false;
        Turn result = _nextActionTurn!;
        _nextActionTurn = TryDrawAction();
        return result;
    }

    public Turn DrawQuestion()
    {
        Fresh = false;
        Card question = _questionsDeck.Draw();
        return new Turn(_config.Texts, _config.ImagesFolder, _config.Texts.QuestionsTag, question.Description);
    }

    public void UpdatePlayers(IEnumerable<Player> players) => _players = new PlayerRepository(players);

    private Turn? TryDrawAction()
    {
        Player player = Fresh ? _players.Current : _players.Next;

        while (_actionDecks.Any())
        {
            ActionDeck deck = _actionDecks.First();
            CardAction? action = deck.TrySelectCardFor(player);
            if (action is not null)
            {
                return new Turn(_config.Texts, _config.ImagesFolder, action.Tag, action.Description,
                    CompanionsSelector.CompanionsInfo, action.ImagePath);
            }
            _actionDecks.RemoveAt(0);
        }

        return null;
    }

    private readonly Config _config;
    private readonly IList<ActionDeck> _actionDecks;
    private readonly QuestionDeck _questionsDeck;
    private PlayerRepository _players;
    private Turn? _nextActionTurn;
}
