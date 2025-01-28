using System.Collections.Generic;
using DaresGameBot.Game.Data;
using DaresGameBot.Game.Matchmaking.ActionCheck;
using DaresGameBot.Game.Players;

namespace DaresGameBot.Game.Decks;

internal sealed class ActionDeck : Deck<ActionData>
{
    public ActionDeck(IReadOnlyList<ActionData> cardDatas, IActionChecker checker) : base(cardDatas)
    {
        _checker = checker;
    }

    public ArrangementType? TrySelectArrangement(Repository players)
    {
        ushort? id = GetRandomId(c => _checker.CanPlay(players.Current, c.ArrangementType));
        if (id is null)
        {
            return null;
        }

        ActionData card = GetCard(id.Value);
        return card.ArrangementType;
    }

    private readonly IActionChecker _checker;
}