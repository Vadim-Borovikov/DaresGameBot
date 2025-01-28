using System;
using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Game.Data;
using DaresGameBot.Game.Matchmaking.ActionCheck;
using DaresGameBot.Game.Players;
using DaresGameBot.Helpers;

namespace DaresGameBot.Game.Decks;

internal sealed class ActionDeck
{
    public IReadOnlyDictionary<ushort, ActionData> CardDatas => _all.AsReadOnly();

    public ActionDeck(Dictionary<ushort, ActionData> cardDatas, IActionChecker checker)
    {
        _all = cardDatas;
        _checker = checker;
        _current = new HashSet<ushort>(_all.Keys);
    }

    public ArrangementType? TrySelectArrangement(Repository players)
    {
        List<ArrangementType> arrangements = _all.Values
                                             .Select(c => c.ArrangementType)
                                             .Where(a => _checker.CanPlay(players.Current, a))
                                             .ToList();
        return arrangements.Count == 0 ? null : RandomHelper.SelectItem(arrangements);
    }

    public ushort SelectCardId(ArrangementType arrangementType, string tag)
    {
        List<ushort> cardIds = _current.Where(id => (_all[id].Tag == tag)
                                                    && (_all[id].ArrangementType == arrangementType))
                                       .ToList();

        if (!cardIds.Any())
        {
            foreach (ushort id in _all.Keys.Where(id => !_current.Contains(id) && (_all[id].Tag == tag)))
            {
                _current.Add(id);
                if (_all[id].ArrangementType == arrangementType)
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

    public void Fold(ushort id) => _current.Remove(id);

    private readonly HashSet<ushort> _current;
    private readonly Dictionary<ushort, ActionData> _all;
    private readonly IActionChecker _checker;
}