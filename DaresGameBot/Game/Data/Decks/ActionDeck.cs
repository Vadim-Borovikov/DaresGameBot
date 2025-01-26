using System;
using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Game.Matchmaking.ActionCheck;
using DaresGameBot.Helpers;

namespace DaresGameBot.Game.Data.Decks;

internal sealed class ActionDeck
{
    public IReadOnlyDictionary<ushort, Cards.Action> Cards => _all.AsReadOnly();
    public readonly IActionChecker Checker;

    public ActionDeck(Dictionary<ushort, Cards.Action> cards, IActionChecker checker)
    {
        _all = cards;
        Checker = checker;
        _current = new HashSet<ushort>(_all.Keys);
    }

    public ushort SelectCard(PlayerRepository players)
    {
        List<ushort> bestIds = GetBestCards(players);
        ushort id = RandomHelper.SelectItem(bestIds);
        _current.Remove(id);
        return id;
    }

    private List<ushort> GetBestCards(PlayerRepository players)
    {
        List<ushort> result = players.EnumerateBestIdsOf(_current).ToList();

        if (!result.Any() && (_current.Count < _all.Count))
        {
            _current = new HashSet<ushort>(_all.Keys);
            result = players.EnumerateBestIdsOf(_current).ToList();
        }

        if (!result.Any())
        {
            throw new Exception("No suitable cards found");
        }

        return result;
    }

    private HashSet<ushort> _current;
    private readonly Dictionary<ushort, Cards.Action> _all;
}