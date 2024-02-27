using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Game.ActionCheck;
using DaresGameBot.Game.Data.Cards;
using DaresGameBot.Game.Data.Players;
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

    public void UpdatePossibilities(IEnumerable<Player> players)
    {
        _possiblePlayers.Clear();
        foreach (Player player in players)
        {
            foreach (CardAction action in _cards)
            {
                if (!_checker.Check(player, action))
                {
                    continue;
                }
                if (!_possiblePlayers.ContainsKey(action.Id))
                {
                    _possiblePlayers[action.Id] = new List<string>();
                }
                _possiblePlayers[action.Id].Add(player.Name);
            }
        }
    }

    public CardAction? TrySelectCardFor(Player player)
    {
        IEnumerable<CardAction> possibleCards = _cards.Where(c => _possiblePlayers.ContainsKey(c.Id)
                                                                  && _possiblePlayers[c.Id].Contains(player.Name));

        List<CardAction>? bestCards = possibleCards.GroupBy(c => _possiblePlayers[c.Id].Count)
                                                   .MinBy(g => g.Key)
                                                   ?.ToList();

        if (bestCards is null || !bestCards.Any())
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