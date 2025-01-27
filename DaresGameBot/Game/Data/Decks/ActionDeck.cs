using System;
using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Game.Data.Cards;
using DaresGameBot.Game.Matchmaking.ActionCheck;
using DaresGameBot.Helpers;

namespace DaresGameBot.Game.Data.Decks;

internal sealed class ActionDeck
{
    public IReadOnlyDictionary<ushort, Cards.Action> Cards => _all.AsReadOnly();

    public ActionDeck(Dictionary<ushort, Cards.Action> cards, IActionChecker checker)
    {
        _all = cards;
        _checker = checker;
        _current = new HashSet<ushort>(_all.Keys);

        _arrangements = _all.Values
                            .Select(a => a.Arrangement)
                            .DistinctBy(a => a.GetHashCode())
                            .ToDictionary(a => a.GetHashCode(), a => a);
    }

    public Arrangement GetArrangement(int hash) => _arrangements[hash];

    public Arrangement? TrySelectArrangement(PlayerRepository players)
    {
        List<Arrangement> arrangements = _all.Values
                                             .Select(c => c.Arrangement)
                                             .Where(a => _checker.CanPlay(players.Current, a))
                                             .ToList();
        return arrangements.Count == 0 ? null : RandomHelper.SelectItem(arrangements);
    }

    public ushort SelectCard(ArrangementInfo arrangementInfo, string tag)
    {
        List<ushort> cardIds = _current.Where(id => (_all[id].Tag == tag)
                                                    && (_all[id].Arrangement.GetHashCode() == arrangementInfo.Hash))
                                       .ToList();

        if (!cardIds.Any())
        {
            foreach (ushort id in _all.Keys.Where(id => !_current.Contains(id) && (_all[id].Tag == tag)))
            {
                _current.Add(id);
                if (_all[id].Arrangement.GetHashCode() == arrangementInfo.Hash)
                {
                    cardIds.Add(id);
                }
            }
        }

        if (!cardIds.Any())
        {
            throw new Exception("No suitable cards found");
        }

        return RandomHelper.SelectItem(cardIds);
    }

    public void FoldCard(ushort id) => _current.Remove(id);

    private readonly HashSet<ushort> _current;
    private readonly Dictionary<ushort, Cards.Action> _all;
    private readonly IActionChecker _checker;
    private readonly Dictionary<int, Arrangement> _arrangements;
}