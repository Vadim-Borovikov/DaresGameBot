using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Game.ActionCheck;
using DaresGameBot.Game.Data.Cards;
using DaresGameBot.Helpers;

namespace DaresGameBot.Game.Data.Decks;

internal sealed class ActionDeck
{
    private readonly IActionChecker _checker;

    public ActionDeck(IEnumerable<CardAction> source, IActionChecker checker)
    {
        _checker = checker;
        CardAction[] cards = RandomHelper.Shuffle(source);
        _cards = cards.ToList();
    }

    public CardAction? TrySelectCardFor(Player player)
    {
        for (int i = 0; i < _cards.Count; ++i)
        {
            CardAction card = _cards[i];
            if (_checker.Check(player, card))
            {
                _cards.RemoveAt(i);
                return card;
            }
        }

        return null;
    }

    private readonly List<CardAction> _cards;
}