using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Game.Data.Cards;
using DaresGameBot.Game.Matchmaking.ActionCheck;
using DaresGameBot.Helpers;

namespace DaresGameBot.Game.Data.Decks;

internal sealed class ActionDeck
{
    private readonly IActionChecker _checker;

    public ActionDeck(IEnumerable<CardAction> source, IActionChecker checker)
    {
        _checker = checker;
        _cards = new List<CardAction>(source);
        _possiblePlayers = new Dictionary<ushort, List<string>>();
    }

    public bool IsEmpty() => !_possiblePlayers.Any();

    public void UpdatePossibilities(IEnumerable<string> players)
    {
        _possiblePlayers.Clear();
        foreach (string player in players)
        {
            foreach (CardAction action in _cards)
            {
                if (!_checker.CanPlay(player, action))
                {
                    continue;
                }
                if (!_possiblePlayers.ContainsKey(action.Id))
                {
                    _possiblePlayers[action.Id] = new List<string>();
                }
                _possiblePlayers[action.Id].Add(player);
            }
        }
    }

    public CardAction? TrySelectCardFor(string player)
    {
        IEnumerable<CardAction> possibleCards = _cards.Where(c => _possiblePlayers.ContainsKey(c.Id)
                                                                  && _possiblePlayers[c.Id].Contains(player));

        List<CardAction> bestCards = possibleCards.GroupBy(c => _possiblePlayers[c.Id].Count)
                                                  .OrderBy(g => g.Key)
                                                  .First()
                                                  .ToList();

        if (!bestCards.Any())
        {
            return null;
        }

        CardAction action = RandomHelper.SelectItem(bestCards);
        _cards.Remove(action);
        _possiblePlayers.Remove(action.Id);
        return action;
    }

    private readonly List<CardAction> _cards;
    private readonly Dictionary<ushort, List<string>> _possiblePlayers;
}