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
    public readonly IActionChecker Checker;
    public ActionDeck(Dictionary<ushort, Cards.Action> cards, IActionChecker checker)
    {
        _all = cards;
        Checker = checker;
        _current = new HashSet<ushort>(_all.Keys);

        _arrangements = _all.Values
                            .Select(a => a.Arrangement)
                            .DistinctBy(a => a.GetHashCode())
                            .ToDictionary(a => a.GetHashCode(), a => a);
    }

    public Arrangement GetArrangement(int hash) => _arrangements[hash];

    public Arrangement? TrySelectArrangement(PlayerRepository players)
    {
        Dictionary<int, ushort> arrangementAmountsInDeck = new();
        string smallestDeckTag = _current.GroupBy(id => _all[id].Tag).OrderBy(g => g.Count()).First().Key;
        foreach (int hash in _current.Select(id => _all[id])
                                     .Where(a => a.Tag == smallestDeckTag)
                                     .Select(a => a.Arrangement.GetHashCode()))
        {
            if (arrangementAmountsInDeck.ContainsKey(hash))
            {
                ++arrangementAmountsInDeck[hash];
            }
            else
            {
                arrangementAmountsInDeck[hash] = 1;
            }
        }

        Dictionary<int, ushort> arrangementAmounts = new();
        foreach (int hash in players.PlayableArrangementsForCurrent)
        {
            arrangementAmounts[hash] =
                arrangementAmountsInDeck.ContainsKey(hash) ? arrangementAmountsInDeck[ hash] : (ushort) 1;
        }

        if (arrangementAmounts.Count == 0)
        {
            return null;
        }

        List<int> arrangements = arrangementAmounts.SelectMany(p => Enumerable.Repeat(p.Key, p.Value)).ToList();

        int selected = RandomHelper.SelectItem(arrangements);
        return _arrangements[selected];
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
    private readonly Dictionary<int, Arrangement> _arrangements;
}